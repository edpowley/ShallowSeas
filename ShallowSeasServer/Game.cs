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

		private bool m_quit = false;
		public void quit() { m_quit = true; }

		enum State { WaitingForPlayers, StartingGame, InGame }
		private State m_state = State.WaitingForPlayers;

		public void addPendingClient(ClientWrapper client)
		{
			lock (m_pendingClients)
			{
				m_pendingClients.Add(client);
				client.addMessageHandler<PlayerJoinRequest>(this, handlePlayerJoinRequest);
			}
		}

		private void broadcastMessageToAllPlayers(Message msg)
		{
			foreach (Player player in m_players)
			{
				player.m_client.sendMessage(msg);
			}
		}

		private void broadcastMessageToAllPlayersExcept(Player except, Message msg)
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

			foreach(Player player in m_players)
			{
				result.Add(player.getInfo());
			}

			return result;
		}

		internal void playerInfoHasChanged(Player player)
		{
			broadcastMessageToAllPlayersExcept(player, new SetPlayerInfo() { Player = player.getInfo() });
		}
		
		private void handlePlayerJoinRequest(ClientWrapper client, PlayerJoinRequest msg)
		{
			Player player = new Player(this, client, msg.PlayerName);
			ShallowSeasServer.log(System.Drawing.Color.Black, "Adding player named {0} with id {1}", player.Name, player.m_id);
			m_players.Add(player);

			player.m_client.sendMessage(new WelcomePlayer() { PlayerId = player.m_id });
			broadcastMessageToAllPlayers(new SetPlayerList() { Players = getPlayerInfoList() });

			m_pendingClients.Remove(client);
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

		private static readonly TimeSpan c_pingInterval = TimeSpan.FromSeconds(5);

		private void pingPlayers()
		{
			if (DateTime.Now - m_lastPingTime > c_pingInterval)
			{
				broadcastMessageToAllPlayers(new Ping());
				m_lastPingTime = DateTime.Now;
			}

			for (int playerIndex = 0; playerIndex < m_players.Count; playerIndex++)
			{
				Player player = m_players[playerIndex];

				if (!player.m_client.Connected)
				{
					ShallowSeasServer.log(System.Drawing.Color.Black, "Removing player {0}", player.m_id);
					m_players.RemoveAt(playerIndex);
					playerIndex--;

					broadcastMessageToAllPlayers(new SetPlayerList() { Players = getPlayerInfoList() });
				}
			}
		}

		public void run()
		{
			m_quit = false;
			while (!m_quit)
			{
				handlePendingClients();
				pingPlayers();
				
				foreach (Player player in m_players)
					player.m_client.pumpMessages();

				Thread.Sleep(0);
			}
		}

		public void readyToStart()
		{
			if (m_state != State.WaitingForPlayers)
			{
				ShallowSeasServer.log(System.Drawing.Color.Red, "Game is already in progress");
				return;
			}

			m_state = State.StartingGame;
			foreach (Player player in m_players)
			{
				player.m_client.addMessageHandler<SceneLoaded>(this, handleSceneLoaded);
				player.m_waitingForSceneLoad = true;
				player.m_client.sendMessage(new ReadyToStart());
			}
		}

		void handleSceneLoaded(ClientWrapper client, SceneLoaded msg)
		{
			Player player = m_players.Single(p => p.m_client == client);
			player.m_waitingForSceneLoad = false;

			if (m_players.All(p => p.m_waitingForSceneLoad == false))
			{
				ShallowSeasServer.log(System.Drawing.Color.Black, "All players are in game");
				startGame();
			}
		}

		void startGame()
		{
			m_state = State.InGame;

			StartMainGame msg = new StartMainGame();
			msg.StartPositions = getStartPositions(new SNVector2(129.5f, 127.5f), 1.0f, m_players.Count).ToList();

			broadcastMessageToAllPlayers(msg);
		}

		IEnumerable<SNVector2> getStartPositions(SNVector2 centre, float radius, int numPlayers)
		{
			if (numPlayers == 0)
				yield break;
			else if (numPlayers == 1)
				yield return centre;
			else
			{
				for (int i=0; i<numPlayers; i++)
				{
					float angle = (float)i / numPlayers * 2.0f * (float)Math.PI;
					yield return centre + radius * new SNVector2((float)Math.Cos(angle), (float)Math.Sin(angle));
				}
			}
		}
	}
}
