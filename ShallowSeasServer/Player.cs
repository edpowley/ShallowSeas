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
		public float m_colourH, m_colourS, m_colourV;

		public bool m_waitingForSceneLoad = false;

		private bool m_gearIsCast = false;
		private float m_castEndTime;

		public Player(Game game, ClientWrapper client, string name)
		{
			m_game = game;
			m_id = Guid.NewGuid().ToString();
			m_client = client;
			Name = name;

			m_client.addMessageHandler<SetPlayerName>(this, handleSetName);
			m_client.addMessageHandler<RequestCourse>(this, handleRequestCourse);
			m_client.addMessageHandler<RequestCastGear>(this, handleCastGear);
		}

		public PlayerInfo getInfo()
		{
			return new PlayerInfo { Id = m_id, Name = Name, ColourH = m_colourH, ColourS = m_colourS, ColourV = m_colourV };
		}

		internal void update()
		{
			if (m_gearIsCast && m_game.CurrentTimestamp >= m_castEndTime)
			{
				// TODO
				// m_client.sendMessage(new NotifyCatch());
			}
		}

		private void handleSetName(ClientWrapper client, SetPlayerName msg)
		{
			Name = msg.NewName;
			m_game.playerInfoHasChanged(this);
		}

		private void handleRequestCourse(ClientWrapper client, RequestCourse msg)
		{
			Log.log(Log.Category.GameEvent, "Player {0} set course {1}", m_id,
				string.Join("; ", (from p in msg.Course select p.ToString()).ToArray())
			);

			SetCourse broadcastMsg = new SetCourse();
			broadcastMsg.PlayerId = m_id;
			broadcastMsg.Course = msg.Course;
			broadcastMsg.StartTime = m_game.CurrentTimestamp;

			m_game.broadcastMessageToAllPlayers(broadcastMsg);
		}

		private void handleCastGear(ClientWrapper client, RequestCastGear msg)
		{
			Log.log(Log.Category.GameEvent, "Player {0} cast gear {1} at {2}", m_id, msg.CastDuration, msg.Position);
			m_gearIsCast = true;
			m_castEndTime = m_game.CurrentTimestamp + msg.CastDuration;
		}
	}
}
