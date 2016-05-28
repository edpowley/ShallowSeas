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

		private const int c_numColours = 64;
		private Color[] m_fishMapColours;

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

			initFishMapColours();

			Timer timer = new Timer();
			timer.Interval = 1000;
			timer.Tick += Timer_Tick;
			timer.Start();
		}

		private void initFishMapColours()
		{
			m_fishMapColours = new Color[c_numColours];
			for (int i = 0; i < c_numColours; i++)
			{
				double fraction = (double)i / (double)c_numColours;

				// Colour formula from cam.vogl.c function fraction2rgb
				double hue = 1.0 - fraction;
				if (hue < 0.0) hue = 0.0;
				if (hue > 1.0) hue = 1.0;
				int huesector = (int)Math.Floor(hue * 5.0);
				double huetune = hue * 5.0 - huesector;
				double mix_up = huetune;
				double mix_do = 1.0 - huetune;
				mix_up = Math.Pow(mix_up, 1.0 / 2.5);
				mix_do = Math.Pow(mix_do, 1.0 / 2.5);
				double r, g, b;
				switch (huesector)
				{
					case 0: r = 1.0; g = mix_up; b = 0.0; break; /* red    to yellow */
					case 1: r = mix_do; g = 1.0; b = 0.0; break; /* yellow to green  */
					case 2: r = 0.0; g = 1.0; b = mix_up; break; /* green  to cyan   */
					case 3: r = 0.0; g = mix_do; b = 1.0; break; /* cyan   to blue   */
					case 4: r = 0.0; g = 0.0; b = mix_do; break; /* blue   to black  */
					default: r = 0.0; g = 0.0; b = 0.0; break;
				}

				m_fishMapColours[i] = Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
			}
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
							int colourIndex = (int)density;
							Color colour;
							if (colourIndex < 0)
								colour = m_fishMapColours[0];
							else if (colourIndex >= m_fishMapColours.Length)
								colour = m_fishMapColours[m_fishMapColours.Length - 1];
							else
								colour = m_fishMapColours[colourIndex];

							for (int px = x * pixelSize; px < (x + 1) * pixelSize; px++)
								for (int py = y * pixelSize; py < (y + 1) * pixelSize; py++)
									bitmap.SetPixel(px, (game.m_mapHeight * pixelSize) - 1 - py, colour);
						}
					}

					box.Image = bitmap;
				}
			}
		}
	}
}
