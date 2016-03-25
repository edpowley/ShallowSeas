using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShallowNet
{
    public abstract class Message
    {
        public Message()
        {
        }

        public static IEnumerable<string> AllPropertyNames
        {
            get
            {
                HashSet<string> names = new HashSet<string>();

                foreach (Type type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (type.IsSubclassOf(typeof(Message)))
                    {
                        foreach (var property in type.GetProperties())
                        {
                            names.Add(property.Name);
                        }
                    }
                }

                return names;
            }
        }
    }

    public abstract class ServerToClientMessage : Message { }
    public abstract class ClientToServerMessage : Message { }

    public class CompoundMessage : Message
    {
        public CompoundMessage() { Messages = new List<Message>(); }
        public CompoundMessage(IEnumerable<Message> messages) { Messages = messages.ToList(); }

        public List<Message> Messages { get; set; }
    }

    public class Ping : Message
    {
        public float Timestamp { get; set; }
    }

    public class PlayerJoinRequest : ClientToServerMessage
    {
        public string PlayerName { get; set; }
    }

    public class PlayerInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public float ColourH { get; set; }
        public float ColourS { get; set; }
        public float ColourV { get; set; }
    }

    public class WelcomePlayer : ServerToClientMessage
    {
        public string PlayerId { get; set; }
        public List<PlayerInfo> Players { get; set; }
    }

    public class PlayerJoined : ServerToClientMessage
    {
        public PlayerInfo Player { get; set; }
        public SNVector2 InitialPos { get; set; }
    }

    public class PlayerLeft : ServerToClientMessage
    {
        public string PlayerId { get; set; }
    }

    public class SceneLoaded : ClientToServerMessage
    {
    }

    public class RequestCourse : ClientToServerMessage
    {
        public List<SNVector2> Course { get; set; }
    }

    public class SetCourse : ServerToClientMessage
    {
        public float StartTime { get; set; }
        public string PlayerId { get; set; }
        public List<SNVector2> Course { get; set; }
    }

    public class RequestCastGear : ClientToServerMessage
    {
        public SNVector2 Position { get; set; }
        public string GearName { get; set; }
        public float CastDuration { get; set; }
        public List<float> CatchMultipliers { get; set; }
        public int MaxCatch { get; set; }
    }

    public class SetPlayerCastingGear : ServerToClientMessage
    {
        public string PlayerId { get; set; }
        public SNVector2 Position { get; set; }
        public string GearName { get; set; }
        public float StartTime { get; set; }
        public float EndTime { get; set; }
    }

    public class NotifyCatch : ServerToClientMessage
    {
        public string PlayerId { get; set; }
        public List<int> FishCaught { get; set; }
    }

    public class RequestAnnounce : ClientToServerMessage
    {
        public string Message { get; set; }
        public SNVector2 Position { get; set; }
    }

    public class Announce : ServerToClientMessage
    {
        public string PlayerId { get; set; }
        public string Message { get; set; }
        public SNVector2 Position { get; set; }
    }

    public class RequestFishDensity : ClientToServerMessage
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class InformFishDensity : ServerToClientMessage
    {
        public int X { get; set; }
        public int Y { get; set; }

        public List<float> Density { get; set; }
    }
}
