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

        public bool m_waitingForSceneLoad = false;

        private List<SNVector2> m_currentCourse = null;
        private float m_courseStartTime;

        private List<int> m_currentCatch = new List<int> { 0, 0, 0 };

        private string m_castGear = null;
        private SNVector2 m_castPos;
        private float m_castStartTime;
        private float m_castEndTime;
        private float m_castMaxCatch;
        private List<float> m_castCatchMultipliers;

        public Player(Game game, ClientWrapper client, string name, SNVector2 initialPos)
        {
            m_game = game;
            m_id = Guid.NewGuid().ToString();
            m_client = client;
            m_currentCourse = new List<SNVector2> { initialPos };
            m_courseStartTime = game.CurrentTimestamp;
            Name = name;

            m_client.addMessageHandler<Ping>(this, handlePing);
            m_client.addMessageHandler<RequestCourse>(this, handleRequestCourse);
            m_client.addMessageHandler<RequestCastGear>(this, handleCastGear);
            m_client.addMessageHandler<RequestAnnounce>(this, handleAnnounce);
            m_client.addMessageHandler<RequestFishDensity>(this, handleRequestFishDensity);
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
                NotifyCatch msg = new NotifyCatch();
                msg.PlayerId = m_id;
                msg.FishCaught = new List<int>();

                int totalFish = 0;
                var density = m_game.getFishDensity((int)m_castPos.x, (int)m_castPos.y);
                for (int i = 0; i < density.Count; i++)
                {
                    int numFish = (int)Math.Round(m_game.m_rnd.NextDouble() * density[i] * (m_castEndTime - m_castStartTime) * m_castCatchMultipliers[i]);
                    msg.FishCaught.Add(numFish);
                    totalFish += numFish;
                }

                while (totalFish > m_castMaxCatch)
                {
                    int i = m_game.m_rnd.Next(msg.FishCaught.Count);
                    if (msg.FishCaught[i] > 0)
                    {
                        msg.FishCaught[i]--;
                        totalFish--;
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    m_currentCatch[i] += msg.FishCaught[i];
                    Log.log(Log.Category.GameEvent, "Player {0} caught {1} fish of type {2}", m_id, msg.FishCaught[i], i);
                }

                m_game.broadcastMessageToAllPlayers(msg);
                m_castGear = null;
                m_castEndTime = 0;
                m_castCatchMultipliers = null;
            }
        }

        private void handleRequestCourse(ClientWrapper client, RequestCourse msg)
        {
            if (m_castGear == null)
            {
                Log.log(Log.Category.GameEvent, "Player {0} set course {1}", m_id,
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
            Log.log(Log.Category.GameEvent, "Player {0} cast gear {1} at {2}", m_id, msg.GearName, msg.Position);
            m_castGear = msg.GearName;
            m_castPos = msg.Position;
            m_castStartTime = m_game.CurrentTimestamp;
            m_castEndTime = m_castStartTime + msg.CastDuration;
            m_castCatchMultipliers = msg.CatchMultipliers;
            m_castMaxCatch = msg.MaxCatch;

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
            Log.log(Log.Category.GameEvent, "Player {0} announces '{1}' at {2}", m_id, msg.Message, msg.Position);

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
                Density = m_game.getFishDensity(msg.X, msg.Y)
            };

            m_client.sendMessage(reply);
        }
    }
}
