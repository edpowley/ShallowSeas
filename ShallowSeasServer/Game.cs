using ShallowNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ShallowSeasServer
{
    class Game
    {
        internal Random m_rnd = new Random();

        private List<ClientWrapper> m_pendingClients = new List<ClientWrapper>();
        private List<Player> m_players = new List<Player>();
        public DateTime m_lastPingTime = DateTime.FromFileTime(0);

        private List<float>[,] m_fishDensity;

        private bool m_quit = false;
        public void quit() { m_quit = true; }

        public float CurrentTimestamp { get; private set; }
        private DateTime? m_startTime = null;

        public void addPendingClient(ClientWrapper client)
        {
            lock (m_pendingClients)
            {
                m_pendingClients.Add(client);
                client.addMessageHandler<PlayerJoinRequest>(this, handlePlayerJoinRequest);
            }
        }

        internal void broadcastMessageToAllPlayers(Message msg)
        {
            foreach (Player player in m_players)
            {
                player.m_client.sendMessage(msg);
            }
        }

        internal void broadcastMessageToAllPlayersExcept(Player except, Message msg)
        {
            foreach (Player player in m_players)
            {
                if (player != except)
                    player.m_client.sendMessage(msg);
            }
        }

        public List<PlayerInfo> getPlayerInfoList()
        {
            List<PlayerInfo> result = new List<PlayerInfo>();

            foreach (Player player in m_players)
            {
                result.Add(player.getInfo());
            }

            return result;
        }

        private void handlePlayerJoinRequest(ClientWrapper client, PlayerJoinRequest msg)
        {
            Player player = new Player(this, client, msg.PlayerName);
            Log.log(Log.Category.GameStatus, "Adding player named {0} with id {1}", player.Name, player.m_id);
            m_players.Add(player);

            updatePlayerColours();

            player.m_client.sendMessage(new WelcomePlayer() { PlayerId = player.m_id, Players = getPlayerInfoList() });
            broadcastMessageToAllPlayersExcept(player, new PlayerJoined() { Player = player.getInfo() });

            m_pendingClients.Remove(client);
        }

        private void updatePlayerColours()
        {
            for (int i = 0; i < m_players.Count; i++)
            {
                var player = m_players[i];
                player.m_colourH = (float)i / (float)m_players.Count;
                player.m_colourS = 1;
                player.m_colourV = 1;
            }
        }

        private void handlePendingClients()
        {
            lock (m_pendingClients)
            {
                for (int clientIndex = 0; clientIndex < m_pendingClients.Count; clientIndex++)
                {
                    ClientWrapper client = m_pendingClients[clientIndex];
                    client.pumpMessages();
                }
            }
        }

        private static readonly TimeSpan c_pingInterval = TimeSpan.FromSeconds(1);

        private void pingPlayers()
        {
            if (DateTime.Now - m_lastPingTime > c_pingInterval)
            {
                broadcastMessageToAllPlayers(new Ping() { Timestamp = CurrentTimestamp });
                m_lastPingTime = DateTime.Now;
            }

            for (int playerIndex = 0; playerIndex < m_players.Count; playerIndex++)
            {
                Player player = m_players[playerIndex];

                if (!player.m_client.Connected)
                {
                    Log.log(Log.Category.GameStatus, "Removing player {0}", player.m_id);
                    m_players.RemoveAt(playerIndex);
                    playerIndex--;

                    updatePlayerColours();
                    broadcastMessageToAllPlayers(new PlayerLeft() { PlayerId = player.m_id });
                }
            }
        }

        public void run()
        {
            startGame();

            m_quit = false;
            while (!m_quit)
            {
                if (m_startTime != null)
                    CurrentTimestamp = (float)(DateTime.Now - m_startTime.Value).TotalSeconds;
                else
                    CurrentTimestamp = -1;

                handlePendingClients();
                pingPlayers();

                foreach (Player player in m_players)
                {
                    player.m_client.pumpMessages();
                    player.update();
                }

                Thread.Sleep(0);
            }
        }

        void handleSceneLoaded(ClientWrapper client, SceneLoaded msg)
        {
            Player player = m_players.Single(p => p.m_client == client);
            player.m_waitingForSceneLoad = false;

            if (m_players.All(p => p.m_waitingForSceneLoad == false))
            {
                Log.log(Log.Category.GameStatus, "All players are in game");
                startGame();
            }
        }

        void startGame()
        {
            loadFishDensityMap();

            m_startTime = DateTime.Now;
        }

        IEnumerable<SNVector2> getStartPositions(SNVector2 centre, float radius, int numPlayers)
        {
            if (numPlayers == 0)
                yield break;
            else if (numPlayers == 1)
                yield return centre;
            else
            {
                for (int i = 0; i < numPlayers; i++)
                {
                    float angle = (float)i / numPlayers * 2.0f * (float)Math.PI;
                    yield return centre + radius * new SNVector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                }
            }
        }

        void loadFishDensityMap()
        {
            Bitmap bmp = new Bitmap("FishDensity.png");

            m_fishDensity = new List<float>[bmp.Width, bmp.Height];
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pixel = bmp.GetPixel(x, bmp.Height - 1 - y);
                    m_fishDensity[x, y] = new List<float>() { pixel.R / 255.0f, pixel.G / 255.0f, pixel.B / 255.0f };
                }
            }
        }

        public List<float> getFishDensity(int x, int y)
        {
            return m_fishDensity[x, y];
        }
    }
}
