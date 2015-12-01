using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ShallowNet;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    internal const int c_gridWidth = 256;
    internal const int c_gridHeight = 256;
    internal const int c_numFishTypes = 3;

    private bool[,] m_isWater = new bool[c_gridWidth, c_gridHeight];
    private List<float>[,] m_fishDensity = new List<float>[c_gridWidth, c_gridHeight];

    public Texture2D FishDensityMap;

    public string[] FishNames = new string[]{ "red fish", "green fish", "blue fish" };

    public Boat m_boatPrefab;

    private Dictionary<string, Boat> m_playerBoats = new Dictionary<string, Boat>();

    internal Boat LocalPlayerBoat { get; private set; }

    public BoatCourseLine CourseLine, DrawingLine;

    public Text m_textTopLeft, m_textTopRight, m_textWaitingForGameStart;
    public NotificationText m_notification;

    public FogCircle m_fogCircle;

    internal bool IsWaitingForStart { get; private set; }

    private float m_timestampOffset = 0;

    internal float CurrentTime { get { return Time.timeSinceLevelLoad + m_timestampOffset; } }

    internal bool isWater(int x, int y)
    {
        if (x < 0 || x >= c_gridWidth || y < 0 || y >= c_gridHeight)
            return false;
        else
            return m_isWater[x,y];
    }

    internal bool isWater(IntVector2 p)
    {
        return isWater(p.X, p.Y);
    }

    public void Awake()
    {
        IsWaitingForStart = true;

        if (Instance != null)
            throw new InvalidOperationException("GameManager should be a singleton");

        Instance = this;

        // If the network manager isn't running, go back to the main menu
        // (shouldn't happen in game, but is useful for testing in the Unity editor)
        if (MyNetworkManager.Instance == null)
            Application.LoadLevel((int)Level.MainMenu);
    }

    public void Start()
    {
        initIsWater();
        initFishDensity();

        var client = MyNetworkManager.Instance.m_client;
        client.addMessageHandler<StartMainGame>(this, handleStartMainGame);
        client.addMessageHandler<SetCourse>(this, handleSetCourse);
        client.addMessageHandler<SetPlayerCastingGear>(this, handleSetCasting);
        client.addMessageHandler<NotifyCatch>(this, handleNotifyCatch);
        client.addMessageHandler<ShallowNet.Ping>(this, handlePing);
        client.sendMessage(new SceneLoaded());
    }

    public void OnDestroy()
    {
        if (MyNetworkManager.Instance != null && MyNetworkManager.Instance.m_client != null)
            MyNetworkManager.Instance.m_client.removeMessageHandlers(this);
        
        if (Instance == this)
            Instance = null;
    }

    private void handlePing(ClientWrapper client, ShallowNet.Ping msg)
    {
        m_timestampOffset = msg.Timestamp - Time.timeSinceLevelLoad;
        Debug.LogFormat("m_timestampOffset = {0}", m_timestampOffset);
    }

    private void handleStartMainGame(ClientWrapper client, StartMainGame msg)
    {
        m_textWaitingForGameStart.enabled = false;
        IsWaitingForStart = false;

        for (int i=0; i<MyNetworkManager.Instance.m_players.Count; i++)
        {
            PlayerInfo player = MyNetworkManager.Instance.m_players [i];
            SNVector2 startPos = msg.StartPositions [i];
            Vector3 startPos3 = new Vector3(startPos.x, 0, startPos.y);

            Boat boat = Util.InstantiatePrefab(m_boatPrefab, startPos3, Quaternion.identity);
            boat.PlayerId = player.Id;
            m_playerBoats.Add(player.Id, boat);
            if (player.Id == MyNetworkManager.Instance.LocalPlayerId)
                LocalPlayerBoat = boat;
        }
    }

    private void handleSetCourse(ClientWrapper client, SetCourse msg)
    {
        Boat boat = m_playerBoats [msg.PlayerId];
        boat.setCourse(msg);
    }

    private void handleSetCasting(ClientWrapper client, SetPlayerCastingGear msg)
    {
        Boat boat = m_playerBoats [msg.PlayerId];
        boat.setCasting(msg);
    }

    private void handleNotifyCatch(ClientWrapper client, NotifyCatch msg)
    {
        GameManager.Instance.m_notification.PutMessage("You caught {0} red fish, {1} green fish and {2} blue fish", msg.FishCaught [0], msg.FishCaught [1], msg.FishCaught [2]);

        for (int i=0; i<msg.FishCaught.Count; i++)
        {
            LocalPlayerBoat.m_catch [i] += msg.FishCaught [i];
        }
    }

    private void initFishDensity()
    {
        for (int x=0; x<c_gridWidth; x++)
        {
            for (int y=0; y<c_gridHeight; y++)
            {
                List<float> density = new List<float>();
                Color pixel = FishDensityMap.GetPixel(x, y);
                density.Add(pixel.r);
                density.Add(pixel.g);
                density.Add(pixel.b);
                m_fishDensity[x, y] = density;
            }
        }
    }

    private void initIsWater()
    {
        Terrain terrain = Terrain.activeTerrain;
        
        for (int x=0; x<c_gridWidth; x++)
        {
            for (int y=0; y<c_gridHeight; y++)
            {
                bool isWater = true;
                
                for (float dx = 0.0f; dx <= 1.0f; dx += 0.5f)
                    for (float dy = 0.0f; dy <= 1.0f; dy += 0.5f)
                        isWater = isWater && (terrain.SampleHeight(new Vector3(x + dx, 0, y + dy)) < 16.0f);
                
                m_isWater[x,y] = isWater;
                
                List<float> density = new List<float>();
                Color pixel = FishDensityMap.GetPixel(x, y);
                density.Add(pixel.r);
                density.Add(pixel.g);
                density.Add(pixel.b);
                m_fishDensity[x, y] = density;
            }
        }

        bool[,] reachable = getReachability(c_gridWidth/2, c_gridHeight/2);

        for (int x=0; x<c_gridWidth; x++)
        {
            for (int y=0; y<c_gridHeight; y++)
            {
                m_isWater[x,y] = m_isWater[x,y] && reachable[x,y];
            }
        }
    }

    private bool[,] getReachability(int startX, int startY)
    {
        bool[,] result = new bool[c_gridWidth, c_gridHeight];
        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(new Vector2(startX, startY));

        while (stack.Count > 0)
        {
            Vector2 p = stack.Pop();
            int x = (int)p.x;
            int y = (int)p.y;
            result[x, y] = true;

            if (x > 0 && m_isWater[x-1, y] && !result[x-1, y])
                stack.Push(new Vector2(x-1, y));

            if (y > 0 && m_isWater[x, y-1] && !result[x, y-1])
                stack.Push(new Vector2(x, y-1));

            if (x < c_gridWidth-1 && m_isWater[x+1, y] && !result[x+1, y])
                stack.Push(new Vector2(x+1, y));

            if (y < c_gridHeight-1 && m_isWater[x, y+1] && !result[x, y+1])
                stack.Push(new Vector2(x, y+1));
        }

        return result;
    }

    internal List<float> getFishDensity(IntVector2 cell)
    {
        return m_fishDensity[cell.X, cell.Y];
    }

    public void Update()
    {
        if (!IsWaitingForStart)
        {
            IntVector2 currentCell = GameManager.Instance.LocalPlayerBoat.CurrentCell;

            var currentCellFishDensity = m_fishDensity [currentCell.X, currentCell.Y];
            m_textTopLeft.text = string.Format("Boat in square {0}\nFish density {1}",
                                      currentCell,
                                      string.Join(", ", (from d in currentCellFishDensity select string.Format("{0:0.00}", d)).ToArray())
            );

            m_textTopRight.text = string.Format("Catch: {0}",
                                       string.Join(", ", (from n in LocalPlayerBoat.m_catch select n.ToString()).ToArray())
                                       );
        }
    }
}
