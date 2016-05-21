using ShallowNet;
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
		private Dictionary<FishType, PictureBox> m_fishMapPictureBoxes = new Dictionary<FishType, PictureBox>();

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
			Log.log(Log.Category.ConsoleCommand, commandLine);

			commandLine = commandLine.Trim();

			if (commandLine == "")
				return;

			List<string> args = commandLine.Split().ToList();
			string command = args[0].ToLower();
			args.RemoveAt(0);

			ShallowSeasServer.executeCommand(command, args);
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			fishMapTableLayout.ColumnCount = EcologicalModel.nspp;
			fishMapTableLayout.RowCount = EcologicalModel.nstage;

			foreach (FishType ft in FishType.All)
			{
				PictureBox picture = new PictureBox();
				picture.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
				picture.SizeMode = PictureBoxSizeMode.Zoom;
				fishMapTableLayout.Controls.Add(picture);
				fishMapTableLayout.SetRow(picture, ft.stage);
				fishMapTableLayout.SetColumn(picture, ft.species);
				m_fishMapPictureBoxes.Add(ft, picture);
			}

			Timer timer = new Timer();
			timer.Interval = 500;
			timer.Tick += Timer_Tick;
			timer.Start();
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			updateFishMaps();
		}

		internal void updateFishMaps()
		{
			const int pixelSize = 4;
			Game game = ShallowSeasServer.s_game;
			if (game != null)
			{
				foreach (FishType ft in FishType.All)
				{
					PictureBox box = m_fishMapPictureBoxes[ft];
					Bitmap bitmap = box.Image as Bitmap;
					if (bitmap == null)
						bitmap = new Bitmap(game.m_mapWidth * pixelSize, game.m_mapHeight * pixelSize);

					for (int x = 0; x < game.m_mapWidth; x++)
					{
						for (int y = 0; y < game.m_mapHeight; y++)
						{
							float density = game.getFishDensity(x, y)[ft];
							int rgb = Math.Max(0, Math.Min((int)(density * 100), 255));
							Color color = Color.FromArgb(rgb, rgb, rgb);

							for (int px = x * pixelSize; px < (x + 1) * pixelSize; px++)
								for (int py = y * pixelSize; py < (y + 1) * pixelSize; py++)
									bitmap.SetPixel(px, (game.m_mapHeight * pixelSize) - 1 - py, color);
						}
					}

					box.Image = bitmap;
				}
			}
		}
	}
}