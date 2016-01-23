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

        internal static MainForm s_mainForm { get; private set; }
        static Game s_game = new Game();

        static void listen(int port)
        {
            Log.log(Log.Category.Network, "About to start listening on port {0}", port);
            s_stopListening = false;
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Log.log(Log.Category.Network, "Now listening");

            while (!s_stopListening)
            {
                if (listener.Pending())
                {
                    Log.log(Log.Category.Network, "Got a connection request");
                    ClientWrapper client = new ClientWrapper(listener.AcceptTcpClient());
                    s_game.addPendingClient(client);
                    Log.log(Log.Category.Network, "Accepted connection request");
                }

                Thread.Sleep(100);
            }

            Log.log(Log.Category.Network, "About to stop listening");
            listener.Stop();
            Log.log(Log.Category.Network, "Stopped listening");
        }

        static internal void executeCommand(string command, List<string> args)
        {
            switch (command)
            {
                case "quit":
                case "exit":
                    s_mainForm.Close();
                    break;

                default:
                    Log.log(Log.Category.Error, "Unknown command '{0}'", command);
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
            DebugLog.s_printFunc = (message => Log.log(Log.Category.Debug, message));

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
