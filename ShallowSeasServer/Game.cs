using ShallowNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ShallowSeasServer
{
	class Game
	{
		private List<ClientWrapper> m_pendingClients = new List<ClientWrapper>();
		private List<Player> m_players = new List<Player>();
		public DateTime m_lastPingTime = DateTime.FromFileTime(0);

		public void addPendingClient(ClientWrapper client)
		{
			lock (m_pendingClients)
			{
				m_pendingClients.Add(client);
			}
		}

		private void broadcastMessageToAllPlayers(Message msg)
		{
			foreach(Player player in m_players)
			{
				player.m_client.sendMessage(msg);
			}
		}

		public List<PlayerInfo> getPlayerInfoList()
		{
			List<PlayerInfo> result = new List<PlayerInfo>();

			foreach(Player player in m_players)
			{
				result.Add(new PlayerInfo() { Id = player.m_id, Name = player.Name });
			}

			return result;
		}

		private void handlePendingClients()
		{
			lock (m_pendingClients)
			{
				for (int clientIndex = 0; clientIndex < m_pendingClients.Count; clientIndex++)
				{
					ClientWrapper client = m_pendingClients[clientIndex];

					PlayerJoinRequest msg = client.popMessage<PlayerJoinRequest>();
					if (msg != null)
					{
						Player player = new Player(this, client, msg.PlayerName);
						Console.WriteLine("Adding player named {0} with id {1}", player.Name, player.m_id);
						m_players.Add(player);

						player.m_client.sendMessage(new WelcomePlayer() { PlayerId = player.m_id });
						broadcastMessageToAllPlayers(new SetPlayerList() { Players = getPlayerInfoList() });

						m_pendingClients.RemoveAt(clientIndex);
						clientIndex--;
					}
				}
			}
		}

		private static readonly TimeSpan c_pingInterval = TimeSpan.FromSeconds(5);

		private void pingPlayers()
		{
			if (DateTime.Now - m_lastPingTime > c_pingInterval)
			{
				broadcastMessageToAllPlayers(new Ping());
			}

			for (int playerIndex = 0; playerIndex < m_players.Count; playerIndex++)
			{
				Player player = m_players[playerIndex];

				if (!player.m_client.Connected)
				{
					Console.WriteLine("Removing player {0}", player.m_id);
					m_players.RemoveAt(playerIndex);
					playerIndex--;

					broadcastMessageToAllPlayers(new SetPlayerList() { Players = getPlayerInfoList() });
				}
			}
		}

		public void run()
		{
			while (true)
			{
				handlePendingClients();
				pingPlayers();

				Thread.Sleep(0);
			}
		}
	}
}
