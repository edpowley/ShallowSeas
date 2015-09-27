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
		private ClientWrapper m_client;
		private string m_name;

		public Player(TcpClient client)
		{
			m_client = new ClientWrapper(client);
			m_name = "Anonymous";

			m_client.sendMessage(new TestMessage() { Text = "Howdy" });
		}

		public void ping()
		{
			m_client.sendMessage(new TestMessage() { Text = "Ping!" });
		}
	}
}
