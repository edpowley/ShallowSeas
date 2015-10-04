﻿using ShallowNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ShallowSeasServer
{
	class ShallowSeasServer
	{
		private const int c_defaultPort = 7777;

		static bool s_stopListening = false;

		static Game s_game = new Game();

		static void listen(int port)
		{
			Console.WriteLine("About to start listening on port {0}", port);
			s_stopListening = false;
			TcpListener listener = new TcpListener(IPAddress.Any, port);
			listener.Start();
			Console.WriteLine("Now listening");

			while (!s_stopListening)
			{
				if (listener.Pending())
				{
					Console.WriteLine("Got a connection request");
					ClientWrapper client = new ClientWrapper(listener.AcceptTcpClient());
					s_game.addPendingClient(client);
					Console.WriteLine("Accepted connection request");
				}

				Thread.Sleep(100);
			}

			Console.WriteLine("About to stop listening");
			listener.Stop();
			Console.WriteLine("Stopped listening");
		}

		static void Main(string[] args)
		{
			ShallowNet.DebugLog.s_printFunc = Console.WriteLine;

			int port = c_defaultPort;
			if (args.Length > 1)
			{
				port = int.Parse(args[1]);
			}

			Thread listenThread = new Thread(new ThreadStart(() => listen(port)));
			listenThread.Start();

			s_game.run();


				/*Console.Write("> ");
				string line = Console.ReadLine();
				string[] commandLine = line.Split(' ');
				string command = commandLine[0].ToLower();

				switch (command)
				{
					case "ping":
						foreach (Player player in s_players)
							player.ping();
						break;

					case "exit":
						exit = true;
						break;

					default:
						Console.WriteLine("Unknown command '{0}'", command);
						break;
				}*/

			s_stopListening = true;
			listenThread.Join();
		}
	}
}
