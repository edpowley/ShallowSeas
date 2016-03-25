using ShallowNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

            writeToLogFile(msg);
        }

        internal void broadcastMessageToAllPlayersExcept(Player except, Message msg)
        {
            foreach (Player player in m_players)
            {
                if (player != except)
                    player.m_client.sendMessage(msg);
            }

            writeToLogFile(msg);
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

        private int m_nextInitialPositionIndex = 0;
        static SNVector2 c_initialPosCentre = new SNVector2(132.5f, 127.5f);

        private SNVector2 getNextInitialPosition()
        {
            float angle = m_nextInitialPositionIndex * (float)Math.PI / 4.0f;
            float radius = 1.0f + m_nextInitialPositionIndex / 8.0f;
            m_nextInitialPositionIndex++;
            return new SNVector2(
                c_initialPosCentre.x + radius * (float)Math.Cos(angle),
                c_initialPosCentre.y + radius * (float)Math.Sin(angle)
            );
        }

        private void handlePlayerJoinRequest(ClientWrapper client, PlayerJoinRequest joinMsg)
        {
            SNVector2 pos = getNextInitialPosition();
            Player player = new Player(this, client, joinMsg.PlayerName, pos);
            Log.log(Log.Category.GameStatus, "Adding player named {0} with id {1} at {2}", player.Name, player.m_id, pos);
            m_players.Add(player);

            updatePlayerColours();

            CompoundMessage welcomeMsg = new CompoundMessage();
            welcomeMsg.Messages.Add(new WelcomePlayer() { PlayerId = player.m_id, Players = getPlayerInfoList() });
            foreach (Player otherPlayer in m_players)
            {
                welcomeMsg.Messages.AddRange(otherPlayer.getStatusSyncMessages());
            }
            player.m_client.sendMessage(welcomeMsg);

            broadcastMessageToAllPlayersExcept(player, new PlayerJoined() { Player = player.getInfo(), InitialPos = pos });

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
                    Log.log(Log.Category.GameStatus, "Removing player {0}", player.Name);
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
            initLogFile();

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

        StreamWriter m_logWriter = null;
        List<string> m_logPropertyNames = null;

        private void initLogFile()
        {
            string filename;
            do
            {
                filename = string.Format("log_{0:yyyy-MM-dd_HH-mm-ss}.txt", DateTime.Now);
            }
            while (File.Exists(filename));

            m_logWriter = new StreamWriter(filename);

            m_logPropertyNames = new List<string>(Message.AllPropertyNames);
            m_logPropertyNames.Sort();
            m_logWriter.Write("_real time_\t_event type_");
            foreach (string propertyName in m_logPropertyNames)
            {
                m_logWriter.Write("\t{0}", propertyName);
            }
            m_logWriter.WriteLine();
            m_logWriter.Flush();
        }

        private void writeToLogFile(Message msg)
        {
            if (msg is CompoundMessage)
            {
                foreach (Message childMsg in (msg as CompoundMessage).Messages)
                {
                    writeToLogFile(childMsg);
                }
            }
            else
            {
                m_logWriter.Write("{0}\t{1}", DateTime.Now, msg.GetType().Name);
                foreach (string propertyName in m_logPropertyNames)
                {
                    var property = msg.GetType().GetProperty(propertyName);
                    if (property != null)
                        m_logWriter.Write("\t{0}", property.GetValue(msg, null));
                    else
                        m_logWriter.Write("\t");
                }
                m_logWriter.WriteLine();
                m_logWriter.Flush();
            }
        }

        private string getWritableValue(object value)
        {
            if (value is IEnumerable)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                bool first = true;
                foreach (object element in (IEnumerable)value)
                {
                    if (!first)
                        sb.Append(", ");
                    first = false;
                    sb.Append(getWritableValue(element));
                }
                sb.Append("]");
                return sb.ToString();
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
