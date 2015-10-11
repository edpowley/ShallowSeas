using ShallowNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ShallowSeasServer
{
	class Player
	{
		private Game m_game;
		public readonly string m_id;
		public ClientWrapper m_client;
		public string Name { get; set; }

		public Player(Game game, ClientWrapper client, string name)
		{
			m_game = game;
			m_id = Guid.NewGuid().ToString();
			m_client = client;
			Name = name;
		}

		public PlayerInfo getInfo()
		{
			return new PlayerInfo { Id = m_id, Name = Name };
		}

		public void handleMessages()
		{
			SetPlayerName msg = m_client.popMessage<SetPlayerName>();
			if (msg != null)
			{
				Name = msg.NewName;
				m_game.playerInfoHasChanged(this);
			}
		}
	}
}
