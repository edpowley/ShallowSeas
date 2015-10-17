using ShallowNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ShallowSeasServer
{
	class ShallowSeasServer
	{
		private const int c_defaultPort = 7777;

		static Thread s_listenThread, s_gameThread;
		static bool s_stopListening = false;

		static MainForm s_mainForm;
		static Game s_game = new Game();

		static internal void log(Color color, string message)
		{
			s_mainForm.logWriteLine(color, message);
		}

		static internal void log(Color color, string format, params object[] args)
		{
			s_mainForm.logWriteLine(color, String.Format(format, args));
		}

		static void listen(int port)
		{
			ShallowSeasServer.log(Color.Black, "About to start listening on port {0}", port);
			s_stopListening = false;
			TcpListener listener = new TcpListener(IPAddress.Any, port);
			listener.Start();
			ShallowSeasServer.log(Color.Black, "Now listening");

			while (!s_stopListening)
			{
				if (listener.Pending())
				{
					ShallowSeasServer.log(Color.Black, "Got a connection request");
					ClientWrapper client = new ClientWrapper(listener.AcceptTcpClient());
					s_game.addPendingClient(client);
					ShallowSeasServer.log(Color.Black, "Accepted connection request");
				}

				Thread.Sleep(100);
			}

			ShallowSeasServer.log(Color.Black, "About to stop listening");
			listener.Stop();
			ShallowSeasServer.log(Color.Black, "Stopped listening");
		}

		static internal void executeCommand(string command, List<string> args)
		{
			switch (command)
			{
				case "start":
					s_game.readyToStart();
					break;

				case "quit":
				case "exit":
					s_mainForm.Close();
					break;

				default:
					ShallowSeasServer.log(Color.Red, "Unknown command '{0}'", command);
					break;
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			s_mainForm = new MainForm();
			DebugLog.s_printFunc = (message => s_mainForm.logWriteLine(Color.Goldenrod, message));

			int port = c_defaultPort;
			s_listenThread = new Thread(new ThreadStart(() => listen(port)));
			s_listenThread.Start();

			s_gameThread = new Thread(new ThreadStart(() => s_game.run()));
			s_gameThread.Start();

			s_mainForm.FormClosing += mainForm_Closing;

			Application.Run(s_mainForm);
		}

		private static void mainForm_Closing(object sender, FormClosingEventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to quit? This will disconnect all players and end the game.", "Shallow Seas Server", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				s_stopListening = true;
				s_game.quit();
				s_listenThread.Join();
				s_gameThread.Join();
			}
			else
			{
				e.Cancel = true;
			}
		}
	}
}
