using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;

namespace ShallowSeasServer
{
	static class Log
	{
		internal enum Category
		{
			Debug,
			GameStatus,
			GameEvent,
			Network,
			ConsoleCommand,
			Error,
		}

		static private Dictionary<Category, Color> s_categoryColours = new Dictionary<Category, Color>
		{
			{ Category.Debug, Color.LightGray },
			{ Category.GameStatus, Color.Navy },
			{ Category.GameEvent, Color.Indigo },
			{ Category.Network, Color.Goldenrod },
			{ Category.ConsoleCommand, Color.Blue },
			{ Category.Error, Color.Red }
		};

		static private StreamWriter s_logWriter = null;

		static Log()
		{
			string filename;
			int i = 0;
			do
			{
				filename = string.Format("log_{0:yyyy-MM-dd_HH-mm-ss}_{1}.html", DateTime.Now, i);
			}
			while (File.Exists(filename));

			s_logWriter = new StreamWriter(filename);
			s_logWriter.WriteLine("<!DOCTYPE html>");
			s_logWriter.WriteLine("<html><head>");
			s_logWriter.WriteLine("<title>Shallow Seas {0}</title>", DateTime.Now);
			s_logWriter.WriteLine("<style type=\"text/css\">");
			s_logWriter.WriteLine(".message { font-family: courier }");
			foreach(var kv in s_categoryColours)
			{
				s_logWriter.WriteLine(".{0} {{ color: #{1:X6} }}", kv.Key, kv.Value.ToArgb() & 0xFFFFFF);
			}
			s_logWriter.WriteLine(".Debug { display: none }");
			s_logWriter.WriteLine(".message>.timestamp:before { content: \"[\"}");
			s_logWriter.WriteLine(".message>.timestamp:after  { content: \"]\"}");
			s_logWriter.WriteLine("</style></head><body>");
			s_logWriter.Flush();
			s_logWriter.AutoFlush = true;

			log(Category.GameStatus, "Writing log file to {0}", filename);

			AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
		}

		private static void OnProcessExit(object sender, EventArgs e)
		{
			if (s_logWriter != null)
			{
				s_logWriter.WriteLine("</body></html>");
				s_logWriter.Close();
				s_logWriter = null;
			}
		}

		static internal void log(Category category, string message)
		{
			lock(s_logWriter)
			{
				Color color;
				if (!s_categoryColours.TryGetValue(category, out color))
					color = Color.DarkGray;

				if (category != Category.Debug)
				{
					ShallowSeasServer.s_mainForm.logWriteLine(color, message);
				}


				if (s_logWriter != null)
				{
					string encodedMsg = message
						.Replace("&", "&amp;")
						.Replace("<", "&lt;")
						.Replace(">", "&gt;")
						.Replace("\r\n", "\n")
						.Replace("\n", "<br />");

					s_logWriter.WriteLine("<div class=\"message {0}\"><span class=\"timestamp\">{1}</span> {2}</div>", category, DateTime.Now, encodedMsg);
				}
			}
		}

		static internal void log(Category category, string format, params object[] args)
		{
			log(category, string.Format(format, args));
		}

	}
}
