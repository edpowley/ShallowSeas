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
		private Game m_game;
		public readonly string m_id;
		public ClientWrapper m_client;
		public string Name { get; set; }
		public float m_colourH, m_colourS, m_colourV;

		private List<SNVector2> m_currentCourse = null;
		private float m_courseStartTime;

		private Dictionary<FishType, int> m_currentCatch;

		private string m_castGear = null;
		private SNVector2 m_castPos;
		private float m_castStartTime;
		private float m_castEndTime;

		internal int m_money;
		internal float m_fuel;
		internal Dictionary<string, int> m_spending;

		internal bool m_isReady = false;

		public Player(Game game, ClientWrapper client, string name)
		{
			m_game = game;
			m_id = Guid.NewGuid().ToString();
			m_client = client;
			m_currentCourse = new List<SNVector2>();
			m_courseStartTime = game.CurrentTimestamp;
			Name = name;
			m_fuel = m_game.m_settings.maxFuel;
			m_money = 0;

			m_spending = new Dictionary<string, int>();
			foreach (var item in m_game.m_settings.buyItems)
			{
				m_spending.Add(item.name, 0);
			}

			m_currentCatch = new Dictionary<FishType, int>();
			foreach (FishType ft in FishType.All)
				m_currentCatch.Add(ft, 0);

			m_client.addMessageHandler<Ping>(this, handlePing);
			m_client.addMessageHandler<RequestCourse>(this, handleRequestCourse);
			m_client.addMessageHandler<RequestCastGear>(this, handleCastGear);
			m_client.addMessageHandler<RequestAnnounce>(this, handleAnnounce);
			m_client.addMessageHandler<RequestFishDensity>(this, handleRequestFishDensity);
			m_client.addMessageHandler<FinishedShopping>(this, handleFinishedShopping);
			m_client.addMessageHandler<RequestBuy>(this, handleBuy);
		}

		internal void resetAtRoundStart(SNVector2 startPos)
		{
			m_currentCourse.Clear();
			m_currentCourse.Add(startPos);
			m_courseStartTime = 0;
			m_fuel = m_game.m_settings.maxFuel;
			m_castGear = null;

			foreach (FishType ft in FishType.All)
				m_currentCatch[ft] = 0;
		}

		internal string sellCatch()
		{
			StringBuilder result = new StringBuilder();
			int gross = 0;
			foreach(FishType ft in FishType.All)
			{
				int count = m_currentCatch[ft];
				int price = m_game.m_settings.fishSpecies[ft.species].prices[ft.stage];
				if (count > 0)
					result.AppendFormat("{0} {1} at ${2} each = ${3}\n", count, ft, price, count * price);
				gross += count * price;
			}
			result.AppendFormat("Total: ${0}", gross);
			m_money += gross;
			return result.ToString();
		}

		private void handlePing(ClientWrapper client, Ping msg)
		{
		}

		public PlayerInfo getInfo()
		{
			return new PlayerInfo { Id = m_id, Name = Name, ColourH = m_colourH, ColourS = m_colourS, ColourV = m_colourV, };
		}

		/** Get the sequence of messages to send to a client to update the status of this player */
		public IEnumerable<Message> getStatusSyncMessages()
		{
			if (m_castGear != null)
			{
				yield return new SetPlayerCastingGear()
				{
					PlayerId = m_id,
					Position = m_castPos,
					GearName = m_castGear,
					StartTime = m_castStartTime,
					EndTime = m_castEndTime
				};
			}
			else if (m_currentCourse != null)
			{
				yield return new SetCourse()
				{
					PlayerId = m_id,
					Course = m_currentCourse,
					StartTime = m_courseStartTime
				};
			}

			yield return new NotifyCatch()
			{
				PlayerId = m_id,
				FishCaught = m_currentCatch
			};
		}

		internal void update()
		{
			if (m_castGear != null && m_game.CurrentTimestamp >= m_castEndTime)
			{
				var gearInfo = m_game.m_settings.gear.Single(g => g.name == m_castGear);

				NotifyCatch msg = new NotifyCatch();
				msg.PlayerId = m_id;
				msg.FishCaught = new Dictionary<FishType, int>();

				int totalFish = 0;
				var density = m_game.getFishDensity((int)m_castPos.x, (int)m_castPos.y);
				float totalDensity = density.Values.Sum();
				float catchSize = Math.Min(totalDensity, gearInfo.maxCatch);
				foreach(FishType ft in FishType.All)
				{
					int numFish = (int)Math.Round(density[ft] / totalDensity * catchSize);
					msg.FishCaught.Add(ft, numFish);
					totalFish += numFish;
				}

				int previousCatch = m_currentCatch.Values.Sum();

				while (totalFish > 0 && (totalFish > gearInfo.maxCatch || previousCatch + totalFish > m_game.m_settings.maxCatch))
				{
					FishType ft = FishType.All[m_game.m_rnd.Next(FishType.All.Count)];
					if (msg.FishCaught[ft] > 0)
					{
						msg.FishCaught[ft]--;
						totalFish--;
					}
				}

				m_game.removeFish((int)m_castPos.x, (int)m_castPos.y, msg.FishCaught);
				
				foreach (FishType ft in FishType.All)
				{
					m_currentCatch[ft] += msg.FishCaught[ft];
					Log.log(Log.Category.GameEvent, "Player {0} caught {1} fish of type {2}", Name, msg.FishCaught[ft], ft);
				}

				m_game.broadcastMessageToAllPlayers(msg);
				m_castGear = null;
				m_castEndTime = 0;
			}
		}

		private void handleRequestCourse(ClientWrapper client, RequestCourse msg)
		{
			if (m_castGear == null)
			{
				Log.log(Log.Category.GameEvent, "Player {0} set course {1}", Name,
					string.Join("; ", (from p in msg.Course select p.ToString()).ToArray())
				);

				m_currentCourse = msg.Course;
				m_courseStartTime = m_game.CurrentTimestamp;

				SetCourse broadcastMsg = new SetCourse();
				broadcastMsg.PlayerId = m_id;
				broadcastMsg.Course = m_currentCourse;
				broadcastMsg.StartTime = m_courseStartTime;

				m_game.broadcastMessageToAllPlayers(broadcastMsg);
			}
		}

		private void handleCastGear(ClientWrapper client, RequestCastGear msg)
		{
			Log.log(Log.Category.GameEvent, "Player {0} cast gear {1} at {2}", Name, msg.GearName, msg.Position);
			m_castGear = msg.GearName;
			var gearInfo = m_game.m_settings.gear.Single(g => g.name == msg.GearName);
			m_castPos = msg.Position;
			m_castStartTime = m_game.CurrentTimestamp;
			m_castEndTime = m_castStartTime + gearInfo.castTime;

			SetPlayerCastingGear broadcastMsg = new SetPlayerCastingGear();
			broadcastMsg.PlayerId = m_id;
			broadcastMsg.Position = msg.Position;
			broadcastMsg.GearName = msg.GearName;
			broadcastMsg.StartTime = m_castStartTime;
			broadcastMsg.EndTime = m_castEndTime;

			m_game.broadcastMessageToAllPlayers(broadcastMsg);
		}

		private void handleAnnounce(ClientWrapper client, RequestAnnounce msg)
		{
			Log.log(Log.Category.GameEvent, "Player {0} announces '{1}' at {2}", Name, msg.Message, msg.Position);

			Announce broadcastMsg = new Announce();
			broadcastMsg.PlayerId = m_id;
			broadcastMsg.Message = msg.Message;
			broadcastMsg.Position = msg.Position;
			m_game.broadcastMessageToAllPlayers(broadcastMsg);
		}

		private void handleRequestFishDensity(ClientWrapper client, RequestFishDensity msg)
		{
			InformFishDensity reply = new InformFishDensity()
			{
				X = msg.X,
				Y = msg.Y,
				Width = msg.Width,
				Height = msg.Height
			};

			byte[] bytes = new byte[msg.Width * msg.Height * 6];
			for (int dx = 0; dx < msg.Width; dx++)
			{
				for (int dy = 0; dy < msg.Height; dy++)
				{
					var density = m_game.getFishDensity(msg.X + dx, msg.Y + dy);
					int byteIndex = (dy * msg.Width + dx) * GameConstants.c_numFishSpecies * GameConstants.c_numFishStages;
					foreach(FishType ft in FishType.All)
					{
						bytes[byteIndex] = (byte)(Math.Max(0, Math.Min(density[ft], 255)));
						byteIndex++;
					}
				}
			}

			reply.Density = Convert.ToBase64String(bytes);

			m_client.sendMessage(reply);
		}

		private void handleFinishedShopping(ClientWrapper client, FinishedShopping msg)
		{
			m_isReady = true;
		}

		private void handleBuy(ClientWrapper client, RequestBuy msg)
		{
			var item = m_game.m_settings.buyItems.Single(x => x.name == msg.Item);
			m_money -= msg.Amount;
			InformBuy reply = new InformBuy() { PlayerId = msg.PlayerId, Item = msg.Item, PlayerMoney = m_money };
			switch (item.category)
			{
				case GameSettings.BuyCategory.Self:
					m_spending[item.name] += msg.Amount;
					reply.ItemSpend = m_spending[item.name];
					m_client.sendMessage(reply);
					break;

				case GameSettings.BuyCategory.Group:
					m_game.m_groupSpend[item.name] += msg.Amount;
					reply.ItemSpend = m_game.m_groupSpend[item.name];
					m_game.broadcastMessageToAllPlayers(reply);
					break;
			}
		}
	}
}
