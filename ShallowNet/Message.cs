using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShallowNet
{
	public abstract class Message
	{
		public Message()
		{
			IsHandled = false;
		}

		internal bool IsHandled { get; set; }
	}

	public abstract class ServerToClientMessage : Message { }
	public abstract class ClientToServerMessage : Message { }

	public class Ping : Message
	{
	}

	public class TestMessage : Message
	{
		public string Text { get; set; }
	}

	public class PlayerJoinRequest : ClientToServerMessage
	{
		public string PlayerName { get; set; }
	}

	public class PlayerInfo
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}

	public class WelcomePlayer : ServerToClientMessage
	{
		public string PlayerId { get; set; }
	}

	public class SetPlayerList : ServerToClientMessage
	{
		public List<PlayerInfo> Players { get; set; }
	}

	public class SetPlayerInfo : ServerToClientMessage
	{
		public PlayerInfo Player { get; set; }
	}

	public class SetPlayerName : ClientToServerMessage
	{
		public string NewName { get; set; }
	}
}
