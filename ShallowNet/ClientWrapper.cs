﻿using fastJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ShallowNet
{
    public class ClientWrapper : IDisposable
    {
		private Thread m_thread;
		private TcpClient m_client;
		private NetworkStream m_stream;
		private List<byte> m_readBuffer = new List<byte>();
		private bool m_disconnecting = false;

		private Queue<Message> m_messagesToSend = new Queue<Message>();
		private Queue<Message> m_messagesReceived = new Queue<Message>();

		public static ClientWrapper Connect(string host, int port)
		{
			TcpClient client = new TcpClient(host, port);
			return new ClientWrapper(client);
		}

		public ClientWrapper(TcpClient client)
		{
			m_client = client;
			m_stream = m_client.GetStream();

			m_thread = new Thread(new ThreadStart(threadFunc));
			m_thread.Start();
		}

		public void Dispose()
		{
			m_disconnecting = true;
			m_thread.Join();

			m_stream.Close();
			m_client.Close();
		}

		public void sendMessage(Message message)
		{
			lock (m_messagesToSend)
			{
				m_messagesToSend.Enqueue(message);
			}
		}

		public Message getMessage()
		{
			lock (m_messagesReceived)
			{
				return m_messagesReceived.Dequeue();
			}
		}

		public IEnumerable<Message> getMessages()
		{
			lock (m_messagesReceived)
			{
				while (m_messagesReceived.Count > 0)
					yield return m_messagesReceived.Dequeue();
			}
		}

		private void threadFunc()
		{
			while (m_client.Connected && !m_disconnecting)
			{
				// Send messages
				lock (m_messagesToSend)
				{
					while (m_messagesToSend.Count > 0)
					{
						Message msg = m_messagesToSend.Peek();
						string str = JSON.ToJSON(msg);
						DebugLog.WriteLine("Sending:");
						DebugLog.WriteLine(str);

						byte[] data = Encoding.UTF8.GetBytes(str);

						try
						{
							m_stream.Write(data, 0, data.Length);
							m_stream.WriteByte(0);
						}
						catch (Exception e)
						{
							DebugLog.WriteLine("Error sending message: {0}", e);
							break; // while (m_messagesToSend.Count > 0)
						}

						m_messagesToSend.Dequeue();
					}
				}

				// Receive messages
				while (m_stream.DataAvailable)
				{
					byte b = (byte)m_stream.ReadByte();
					if (b != 0)
					{
						m_readBuffer.Add(b);
					}
					else
					{
						string str = Encoding.UTF8.GetString(m_readBuffer.ToArray());
						DebugLog.WriteLine("Received:");
						DebugLog.WriteLine(str);
						m_readBuffer.Clear();

						Message msg = JSON.ToObject<Message>(str);
						DebugLog.WriteLine("Received message {0}", msg);

						lock (m_messagesReceived)
						{
							m_messagesReceived.Enqueue(msg);
						}
					}
				}
			}

			DebugLog.WriteLine("Client disconnected");
		}
	}
}
