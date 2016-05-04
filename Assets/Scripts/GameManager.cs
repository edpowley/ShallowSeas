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

    internal const int c_numFishTypes = 3;

	internal int MapWidth { get; private set; }
	internal int MapHeight { get; private set; }
	private bool[,] m_isWater;
    private List<float>[,] m_fishDensity;

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
        if (x < 0 || x >= MapWidth || y < 0 || y >= MapHeight)
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
        if (MyNetworkManager.Instance == null)
            StartCoroutine(returnToMainMenuAfterDelay(0.1f));
    }

    private IEnumerator returnToMainMenuAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene((int)Level.MainMenu);
    }

    public void Start()
    {
        initMap();

        foreach (PlayerInfo player in MyNetworkManager.Instance.m_players)
        {
            createBoat(player, new SNVector2(0, 0));
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

    private void createBoat(PlayerInfo player, SNVector2 pos)
    {
        Vector3 startPos3;
        startPos3 = new Vector3(pos.x, 0, pos.y);

        Boat boat = Util.InstantiatePrefab(m_boatPrefab, startPos3, Quaternion.identity);
        boat.PlayerId = player.Id;
        m_playerBoats.Add(player.Id, boat);
        if (player.Id == MyNetworkManager.Instance.LocalPlayerId)
            LocalPlayerBoat = boat;
    }

    private void handlePlayerJoined(ClientWrapper client, PlayerJoined msg)
    {
        createBoat(msg.Player, msg.InitialPos);
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
        Debug.LogFormat("Setting density at {0},{1} to {2}", msg.X, msg.Y, msg.Density);
        m_fishDensity[msg.X, msg.Y] = msg.Density;
    }

	private bool[,] getMapWaterFromBase64(WelcomePlayer msg)
	{
		byte[] bytes = Convert.FromBase64String(msg.MapWater);
		bool[,] result = new bool[msg.MapWidth, msg.MapHeight];

		for (int x = 0; x < msg.MapWidth; x++)
		{
			for (int y = 0; y < msg.MapHeight; y++)
			{
				int bitIndex = y * msg.MapWidth + x;
				int byteIndex = bitIndex / 8;
				bitIndex = bitIndex % 8;
				byte mask = (byte)(1 << bitIndex);
				result[x, y] = ((bytes[byteIndex] & mask) == mask);
			}
		}

		return result;
	}

    private void initMap()
    {
        Terrain terrain = Terrain.activeTerrain;
		WelcomePlayer welcome = MyNetworkManager.Instance.m_welcomeMsg;

		MapWidth = welcome.MapWidth;
		MapHeight = welcome.MapHeight;
		m_isWater = getMapWaterFromBase64(welcome);
		m_fishDensity = new List<float>[MapWidth, MapHeight];

		terrain.terrainData.size = new Vector3(welcome.MapWidth, terrain.terrainData.size.y, welcome.MapHeight);

		float[,] heightMap = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
		for(int hx=0;hx< terrain.terrainData.heightmapWidth;hx++)
		{
			int wx = (int)((double)hx / terrain.terrainData.heightmapWidth *welcome.MapWidth);
			for(int hy=0;hy< terrain.terrainData.heightmapHeight;hy++)
			{
				int wy = (int)((double)hy / terrain.terrainData.heightmapHeight * welcome.MapHeight);
				heightMap[hy, hx] = 0.5f + (m_isWater[wx, wy] ? -1.0f : +1.0f) * 0.02f + UnityEngine.Random.Range(-1.0f, +1.0f) * 0.001f;
			}
		}

		terrain.terrainData.SetHeights(0, 0, heightMap);
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
