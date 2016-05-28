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

		internal GameSettings m_settings;

        private List<ClientWrapper> m_pendingClients = new List<ClientWrapper>();
        private List<Player> m_players = new List<Player>();
        public DateTime m_lastPingTime = DateTime.FromFileTime(0);

		internal int m_mapWidth { get; private set; }
		internal int m_mapHeight { get; private set; }
		private bool[,] m_isWater;
		private SNVector2 m_startCell;

		private EcologicalModel m_ecologicalModel;

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

        private SNVector2 getNextInitialPosition()
        {
            float angle = m_nextInitialPositionIndex * (float)Math.PI / 4.0f;
			float radius = Math.Min(m_nextInitialPositionIndex / 16.0f, 1.0f);
            m_nextInitialPositionIndex++;
            return new SNVector2(
                m_startCell.x + radius * (float)Math.Cos(angle),
				m_startCell.y + radius * (float)Math.Sin(angle)
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
			welcomeMsg.Messages.Add(new WelcomePlayer() {
				PlayerId = player.m_id,
				Players = getPlayerInfoList(),
				Settings = m_settings,
				MapWidth = m_mapWidth,
				MapHeight = m_mapHeight,
				MapWater = getMapWaterAsBase64()
			});
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

			float lastModelUpdate = 0;

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

				if (CurrentTimestamp - lastModelUpdate > 1)
				{
					m_ecologicalModel.iterate();
					lastModelUpdate = CurrentTimestamp;
				}

				Thread.Sleep(1000 / 60);
            }
        }

        void startGame()
        {
			string settingsText = System.IO.File.ReadAllText("GameSettings.json");
			m_settings = fastJSON.JSON.ToObject<GameSettings>(settingsText);

            initLogFile();
			loadMap();
			m_ecologicalModel = new EcologicalModel(m_mapWidth, m_mapHeight, m_isWater);
            m_startTime = DateTime.Now;
        }

		internal void resetEcology()
		{
			m_ecologicalModel = new EcologicalModel(m_mapWidth, m_mapHeight, m_isWater);
		}

		void loadMap()
		{
			Bitmap bmp = new Bitmap("water.png");
			m_mapWidth = bmp.Width;
			m_mapHeight = bmp.Height;
			m_isWater = new bool[m_mapWidth, m_mapHeight];

			for (int x = 0; x < m_mapWidth; x++)
			{
				for (int y = 0; y < m_mapHeight; y++)
				{
					Color pixel = bmp.GetPixel(x, m_mapHeight - 1 - y);
					m_isWater[x, y] = (pixel.B > 128);
					if (pixel.R < 128 && pixel.G > 128 && pixel.B < 128) // green
					{
						m_isWater[x, y] = true;
						m_startCell = new SNVector2(x + 0.5f, y + 0.5f);
					}
				}
			}
		}

		string getMapWaterAsBase64()
		{
			byte[] bytes = new byte[m_mapWidth * m_mapHeight / 8 + 1];
			for (int x = 0; x < m_mapWidth; x++)
			{
				for (int y = 0; y < m_mapHeight; y++)
				{
					if (m_isWater[x, y])
					{
						int bitIndex = y * m_mapWidth + x;
						int byteIndex = bitIndex / 8;
						bitIndex = bitIndex % 8;
						byte mask = (byte)(1 << bitIndex);
						bytes[byteIndex] |= mask;
					}
				}
			}
			return Convert.ToBase64String(bytes);
		}

		public Dictionary<FishType, float> getFishDensity(int x, int y)
		{
			Dictionary<FishType, float> result = new Dictionary<FishType, float>();
			foreach (FishType ft in FishType.All)
			{
				if (x >= 0 && x < m_mapWidth && y >= 0 && y < m_mapHeight)
					result.Add(ft, (float)m_ecologicalModel.getDensity(ft.species, ft.stage, x, y));
				else
					result.Add(ft, 0.0f);
			}
			return result;
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
