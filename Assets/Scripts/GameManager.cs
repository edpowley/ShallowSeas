using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using ShallowNet;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    internal const int c_gridWidth = 256;
    internal const int c_gridHeight = 256;
    internal const int c_numFishTypes = 3;

    private bool[,] m_isWater = new bool[c_gridWidth, c_gridHeight];
    private List<float>[,] m_fishDensity = new List<float>[c_gridWidth, c_gridHeight];

    public string[] FishNames = new string[] { "red fish", "green fish", "blue fish" };

    public Boat m_boatPrefab;

    private Dictionary<string, Boat> m_playerBoats = new Dictionary<string, Boat>();

    internal Boat LocalPlayerBoat { get; private set; }

    public BoatCourseLine CourseLine, DrawingLine;

    public Text m_textTopLeft, m_textTopRight;
    public NotificationText m_notification;

    public FogCircle m_fogCircle;

    public float m_timestampOffset = 0;
    private float m_timestampOffsetTarget = 0;

    [Range(0, 1)]
    public float m_timestampOffsetSmoothing = 0.5f;

    internal float CurrentTime { get { return Time.timeSinceLevelLoad + m_timestampOffset; } }

    public TextLabel m_chatPopupPrefab;

    internal bool isWater(int x, int y)
    {
        if (x < 0 || x >= c_gridWidth || y < 0 || y >= c_gridHeight)
            return false;
        else
            return m_isWater[x, y];
    }

    internal bool isWater(IntVector2 p)
    {
        return isWater(p.X, p.Y);
    }

    public void Awake()
    {
        if (Instance != null)
            throw new InvalidOperationException("GameManager should be a singleton");

        Instance = this;

        // If the network manager isn't running, go back to the main menu
        // (shouldn't happen in game, but is useful for testing in the Unity editor)
        //  if (MyNetworkManager.Instance == null)
        //      StartCoroutine(returnToMainMenuAfterDelay(0.1f));
    }

    private IEnumerator returnToMainMenuAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene((int)Level.MainMenu);
    }

    public void Start()
    {
        initIsWater();

        foreach (PlayerInfo player in MyNetworkManager.Instance.m_players)
        {
            createBoat(player);
        }

        var client = MyNetworkManager.Instance.m_client;
        client.addMessageHandler<SetCourse>(this, handleSetCourse);
        client.addMessageHandler<SetPlayerCastingGear>(this, handleSetCasting);
        client.addMessageHandler<NotifyCatch>(this, handleNotifyCatch);
        client.addMessageHandler<Announce>(this, handleAnnounce);
        client.addMessageHandler<ShallowNet.Ping>(this, handlePing);
        client.addMessageHandler<InformFishDensity>(this, handleInformFishDensity);
        client.addMessageHandler<PlayerJoined>(this, handlePlayerJoined);
        client.addMessageHandler<PlayerLeft>(this, handlePlayerLeft);
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
        m_timestampOffsetTarget = msg.Timestamp - Time.timeSinceLevelLoad;
        Debug.LogFormat("m_timestampOffsetTarget = {0}", m_timestampOffsetTarget);
    }

    private void createBoat(PlayerInfo player)
    {
        //SNVector2 startPos =  msg.StartPositions[i];
        Vector3 startPos3 = new Vector3(128, 0, 128);

        Boat boat = Util.InstantiatePrefab(m_boatPrefab, startPos3, Quaternion.identity);
        boat.PlayerId = player.Id;
        m_playerBoats.Add(player.Id, boat);
        if (player.Id == MyNetworkManager.Instance.LocalPlayerId)
            LocalPlayerBoat = boat;
    }

    private void handlePlayerJoined(ClientWrapper client, PlayerJoined msg)
    {
        createBoat(msg.Player);
    }

    private void handlePlayerLeft(ClientWrapper client, PlayerLeft msg)
    {
        Boat boat = null;
        if (m_playerBoats.TryGetValue(msg.PlayerId, out boat))
        {
            DestroyObject(boat.gameObject);
            m_playerBoats.Remove(msg.PlayerId);
        }
    }

    private void handleSetCourse(ClientWrapper client, SetCourse msg)
    {
        Boat boat = m_playerBoats[msg.PlayerId];
        boat.setCourse(msg);
    }

    private void handleSetCasting(ClientWrapper client, SetPlayerCastingGear msg)
    {
        Boat boat = m_playerBoats[msg.PlayerId];
        boat.setCasting(msg);
    }

    private void handleNotifyCatch(ClientWrapper client, NotifyCatch msg)
    {
        Boat boat = m_playerBoats[msg.PlayerId];

        for (int i = 0; i < msg.FishCaught.Count; i++)
        {
            boat.m_catch[i] += msg.FishCaught[i];
        }

        if (boat.isLocalPlayer)
        {
            RequestAnnounce announceMsg = new RequestAnnounce();
            announceMsg.Message = string.Format("{0} caught {1} red fish, {2} green fish and {3} blue fish using {4}",
                MyNetworkManager.Instance.getPlayerInfo(boat.PlayerId).Name,
                msg.FishCaught[0], msg.FishCaught[1], msg.FishCaught[2],
                boat.m_castGear);
            announceMsg.Position = new SNVector2(boat.transform.position.x, boat.transform.position.z);

            GameManager.Instance.m_notification.PutMessage(
                () => { MyNetworkManager.Instance.m_client.sendMessage(announceMsg); },
                "You caught {0} red fish, {1} green fish and {2} blue fish", msg.FishCaught[0], msg.FishCaught[1], msg.FishCaught[2]);
        }
    }

    private void handleAnnounce(ClientWrapper client, Announce msg)
    {
        TextLabel popup = Util.InstantiatePrefab(m_chatPopupPrefab);
        popup.transform.position = new Vector3(msg.Position.x, 0, msg.Position.y);
        popup.ShowMessage(msg.Message, 30);
    }

    private void handleInformFishDensity(ClientWrapper client, InformFishDensity msg)
    {
        foreach (var item in msg.Density)
        {
            Debug.LogFormat("Setting density at {0},{1} to {2}", item.x, item.y, item.fish);
            m_fishDensity[item.x, item.y] = item.fish;
        }
    }

    private void initIsWater()
    {
        Terrain terrain = Terrain.activeTerrain;

        for (int x = 0; x < c_gridWidth; x++)
        {
            for (int y = 0; y < c_gridHeight; y++)
            {
                bool isWater = true;

                for (float dx = 0.0f; dx <= 1.0f; dx += 0.5f)
                    for (float dy = 0.0f; dy <= 1.0f; dy += 0.5f)
                        isWater = isWater && (terrain.SampleHeight(new Vector3(x + dx, 0, y + dy)) < 16.0f);

                m_isWater[x, y] = isWater;
            }
        }

        bool[,] reachable = getReachability(c_gridWidth / 2, c_gridHeight / 2);

        for (int x = 0; x < c_gridWidth; x++)
        {
            for (int y = 0; y < c_gridHeight; y++)
            {
                m_isWater[x, y] = m_isWater[x, y] && reachable[x, y];
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

            if (x > 0 && m_isWater[x - 1, y] && !result[x - 1, y])
                stack.Push(new Vector2(x - 1, y));

            if (y > 0 && m_isWater[x, y - 1] && !result[x, y - 1])
                stack.Push(new Vector2(x, y - 1));

            if (x < c_gridWidth - 1 && m_isWater[x + 1, y] && !result[x + 1, y])
                stack.Push(new Vector2(x + 1, y));

            if (y < c_gridHeight - 1 && m_isWater[x, y + 1] && !result[x, y + 1])
                stack.Push(new Vector2(x, y + 1));
        }

        return result;
    }

    internal List<float> getFishDensity(int x, int y)
    {
        return m_fishDensity[x, y];
    }

    internal List<float> getFishDensity(IntVector2 cell)
    {
        return m_fishDensity[cell.X, cell.Y];
    }

    public void Update()
    {
        m_timestampOffset = Mathf.Lerp(m_timestampOffset, m_timestampOffsetTarget, m_timestampOffsetSmoothing);

        IntVector2 currentCell = GameManager.Instance.LocalPlayerBoat.CurrentCell;

        var currentCellFishDensity = m_fishDensity[currentCell.X, currentCell.Y];
        string densityString = "???";
        if (currentCellFishDensity != null)
            densityString = string.Join(", ", (from d in currentCellFishDensity select string.Format("{0:0.00}", d)).ToArray());

        m_textTopLeft.text = string.Format("Boat in square {0}\nFish density {1}", currentCell, densityString);

        m_textTopRight.text = string.Format("Catch: {0}",
                                    string.Join(", ", (from n in LocalPlayerBoat.m_catch select n.ToString()).ToArray())
                                    );
    }
}
