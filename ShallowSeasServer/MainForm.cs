using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShallowSeasServer
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			var foo = this.Handle;
		}

		private void commandBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;

				string cmd = commandBox.Text;
				commandBox.Text = "";
				executeCommand(cmd);
			}
		}

		private void logBox_Enter(object sender, EventArgs e)
		{
			commandBox.Focus();
		}

		public void logWriteLine(Color color, string message)
		{
			if (InvokeRequired)
			{
				logBox.BeginInvoke(new Action(() => logWriteLine(color, message)));
			}
			else
			{
				if (message.Contains("\n"))
				{
					string[] messageLines = message.Split('\n');
					foreach (string line in messageLines)
					{
						logWriteLine(color, line);
					}
				}
				else
				{
					logBox.SelectionColor = color;
					logBox.AppendText(string.Format("[{0}] {1}\n", DateTime.Now, message));
					logBox.ScrollToCaret();
				}
			}
        }

		private void executeCommand(string commandLine)
		{
			logWriteLine(Color.Blue, commandLine);

			commandLine = commandLine.Trim();

			if (commandLine == "")
				return;

			List<string> args = commandLine.Split().ToList();
			string command = args[0].ToLower();
			args.RemoveAt(0);

			ShallowSeasServer.executeCommand(command, args);
		}
	}
}
