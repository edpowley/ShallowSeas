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

        public bool Connected { get; private set; }

        private Queue<Message> m_messagesToSend = new Queue<Message>();
        private List<Message> m_messagesReceived = new List<Message>();

        private abstract class MessageHandler
        {
            protected MessageHandler(object owner, Type type)
            {
                m_owner = owner;
                m_type = type;
            }

            public object m_owner;
            public Type m_type;
            public abstract void handleMessage(ClientWrapper client, Message msg);
        }

        private class MessageHandler<T> : MessageHandler where T : Message
        {
            public MessageHandler(object owner, Action<ClientWrapper, T> func) : base(owner, typeof(T))
            {
                m_func = func;
            }

            private Action<ClientWrapper, T> m_func;
            public override void handleMessage(ClientWrapper client, Message msg)
            {
                if (msg is T)
                    m_func(client, (T)msg);
            }
        }

        private List<MessageHandler> m_messageHandlers = new List<MessageHandler>();

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

        public void addMessageHandler<MessageType>(object owner, Action<ClientWrapper, MessageType> handler) where MessageType : Message
        {
            m_messageHandlers.Add(new MessageHandler<MessageType>(owner, handler));
        }

        public void removeMessageHandlers(object owner)
        {
            m_messageHandlers.RemoveAll(handler => handler.m_owner == owner);
        }

        public void pumpMessages()
        {
            lock (m_messagesReceived)
            {
                foreach (Message msg in m_messagesReceived)
                {
                    bool handled = false;
                    List<MessageHandler> handlers = new List<MessageHandler>(m_messageHandlers);
                    foreach (MessageHandler handler in handlers)
                    {
                        if (handler.m_type.IsInstanceOfType(msg))
                        {
                            handler.handleMessage(this, msg);
                            handled = true;
                        }
                    }

                    if (!handled)
                    {
                        DebugLog.WriteLine("WARNING: No handler found for message {0}", msg);
                    }
                }

                m_messagesReceived.Clear();
            }
        }

        private void threadFunc()
        {
            Connected = true;

            while (m_client.Connected && !m_disconnecting)
            {
                // Send messages
                lock (m_messagesToSend)
                {
                    while (m_messagesToSend.Count > 0)
                    {
                        Message msg = m_messagesToSend.Peek();
                        string str = JSON.ToJSON(msg);
                        DebugLog.WriteLine("Sending: {0}", str);

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
                        DebugLog.WriteLine("Received: {0}", str);
                        m_readBuffer.Clear();

                        Message msg = JSON.ToObject<Message>(str);
                        DebugLog.WriteLine("Received message {0}", msg);

                        lock (m_messagesReceived)
                        {
                            m_messagesReceived.Add(msg);
                        }
                    }
                }
            }

            Connected = false;
            DebugLog.WriteLine("Client disconnected");
        }
    }
}
