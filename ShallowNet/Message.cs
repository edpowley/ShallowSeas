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
    }

    public abstract class ServerToClientMessage : Message { }
    public abstract class ClientToServerMessage : Message { }

    public class Ping : Message
    {
        public float Timestamp { get; set; }
    }

    public class TestMessage : Message
    {
        public string Text { get; set; }
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
    }

    public class SetPlayerList : ServerToClientMessage
    {
        public List<PlayerInfo> Players { get; set; }
    }

    public class SetPlayerInfo : ServerToClientMessage
    {
        public PlayerInfo Player { get; set; }
    }

    public class SetPlayerName : ClientToServerMessage
    {
        public string NewName { get; set; }
    }

    public class ReadyToStart : ServerToClientMessage
    {
    }

    public class SceneLoaded : ClientToServerMessage
    {
    }

    public class StartMainGame : ServerToClientMessage
    {
        public List<SNVector2> StartPositions { get; set; }
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
}
