using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ShallowNet;
using UnityEngine.SceneManagement;

public class MyNetworkManager : MonoBehaviour
{
    public static MyNetworkManager Instance { get; private set; }

    internal ClientWrapper m_client = null;

	internal WelcomePlayer m_welcomeMsg = null;
	internal StartRound m_startRoundMsg = null;
	internal StartShop m_startShopMsg = null;

	internal string LocalPlayerId { get; private set; }
    internal List<PlayerInfo> m_players = new List<PlayerInfo>();

    internal PlayerInfo LocalPlayer { get { return m_players.Single(p => p.Id == LocalPlayerId); } }

    public bool IsConnected { get { return m_client != null && m_client.Connected; } }

    public void Awake()
    {
		if (ShallowNet.DebugLog.c_verbose)
			ShallowNet.DebugLog.s_printFunc = Debug.Log;
		else
			ShallowNet.DebugLog.s_printFunc = (msg => { });

        Debug.LogFormat("{0} Awake", this.GetInstanceID());
        if (Instance != null)
        {
            Debug.LogWarning("Already have an instance of MyNetworkManager, destroying this one");
            DestroyObject(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
        StartCoroutine(pingServer());
    }

    public void OnDestroy()
    {
        if (m_client != null)
        {
            m_client.Dispose();
            m_client = null;
        }

        if (Instance == this)
            Instance = null;
    }

    public void JoinServer(string hostName, int port, string playerName)
    {
        if (m_client != null)
            m_client.Dispose();

        m_client = ClientWrapper.Connect(hostName, port);
        m_client.addMessageHandler<ShallowNet.Ping>(this, handlePing);
        m_client.addMessageHandler<WelcomePlayer>(this, handleWelcomeMessage);
		m_client.addMessageHandler<StartRound>(this, handleStartRound);
		m_client.addMessageHandler<StartShop>(this, handleStartShop);
		m_client.addMessageHandler<PlayerJoined>(this, handlePlayerJoined);
        m_client.addMessageHandler<PlayerLeft>(this, handlePlayerLeft);

        m_client.sendMessage(new PlayerJoinRequest() { PlayerName = playerName });
    }

    void handlePing(ClientWrapper client, ShallowNet.Ping msg)
    {
    }

    private void handleWelcomeMessage(ClientWrapper client, WelcomePlayer msg)
    {
        Debug.LogFormat("Joined as player id {0}", msg.PlayerId);
        LocalPlayerId = msg.PlayerId;
        m_players = msg.Players;
		m_welcomeMsg = msg;
    }

	private void handleStartRound(ClientWrapper client, StartRound msg)
	{
		m_startRoundMsg = msg;
		m_startShopMsg = null;
		SceneManager.LoadScene((int)Level.MainGame);
	}

	private void handleStartShop(ClientWrapper client, StartShop msg)
	{
		m_startRoundMsg = null;
		m_startShopMsg = msg;
		SceneManager.LoadScene((int)Level.ShopMenu);
	}

	private void handlePlayerJoined(ClientWrapper client, PlayerJoined msg)
    {
        m_players.Add(msg.Player);
    }

    private void handlePlayerLeft(ClientWrapper client, PlayerLeft msg)
    {
        int index = m_players.FindIndex(p => p.Id == msg.PlayerId);
        if (index != -1)
            m_players.RemoveAt(index);
    }

    public void Update()
    {
        if (m_client != null)
        {
            m_client.pumpMessages();
        }
    }

    private IEnumerator pingServer()
    {
        while (true)
        {
            if (IsConnected)
            {
                m_client.sendMessage(new ShallowNet.Ping());

                if (!IsConnected)
                {
                    Debug.LogWarning("Disconnected from server");
                }
            }

            yield return new WaitForSeconds(1.9f);
        }
    }

    internal PlayerInfo getPlayerInfo(string id)
    {
        return m_players.SingleOrDefault(p => p.Id == id);
    }
}
