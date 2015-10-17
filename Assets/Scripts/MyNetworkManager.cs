using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using ShallowNet;

public class MyNetworkManager : MonoBehaviour
{
    public static MyNetworkManager Instance { get; private set; }

    internal ClientWrapper m_client = null;

    internal string LocalPlayerId { get; private set; }
    internal List<PlayerInfo> m_players = new List<PlayerInfo>();

    internal PlayerInfo LocalPlayer{ get { return m_players.Single(p => p.Id == LocalPlayerId); } }

    public bool IsConnected { get { return m_client != null; } }

    public void Awake()
    {
        ShallowNet.DebugLog.s_printFunc = Debug.Log;

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

    public void JoinServer(string hostName, int port)
    {
        if (m_client != null)
            m_client.Dispose();

        m_client = ClientWrapper.Connect(hostName, port);
        m_client.addMessageHandler<SetPlayerList>(this, handleSetPlayerList);
        m_client.addMessageHandler<SetPlayerInfo>(this, handleSetPlayerInfo);
        m_client.addMessageHandler<WelcomePlayer>(this, handleWelcomeMessage);
        m_client.addMessageHandler<ReadyToStart>(this, handleReadyToStart);
    }

    void handleSetPlayerList(ClientWrapper client, SetPlayerList msg)
    {
        m_players = msg.Players;
    }
    
    void handleSetPlayerInfo(ClientWrapper client, SetPlayerInfo msg)
    {
        int index = m_players.FindIndex(p => p.Id == msg.Player.Id);
        if (index != -1)
        {
            m_players [index] = msg.Player;
        }
        else
        {
            Debug.LogErrorFormat("Player id {0} not in players list", msg.Player.Id);
        }
    }

    private void handleWelcomeMessage(ClientWrapper client, WelcomePlayer msg)
    {
        Debug.LogFormat("Joined as player id {0}", msg.PlayerId);
        MyNetworkManager.Instance.LocalPlayerId = msg.PlayerId;
    }
    
    private void handleReadyToStart(ClientWrapper client, ReadyToStart msg)
    {
        Application.LoadLevel((int)Level.MainGame);
    }

    public void ChangeLevel(Level level)
    {
        //ServerChangeScene(level.ToString());
    }

    /*public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("OnServerAddPlayer");
        base.OnServerAddPlayer(conn, playerControllerId);
        MyNetworkPlayer.updatePlayers();
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        Debug.Log("OnServerRemovePlayer");
        base.OnServerRemovePlayer(conn, player);
        MyNetworkPlayer.updatePlayers();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnServerDisconnect");
        base.OnServerDisconnect(conn);
        MyNetworkPlayer.updatePlayers();
    }*/

    public void Update()
    {
        if (m_client != null)
        {
            m_client.pumpMessages();
        }
    }

    internal PlayerInfo getPlayerInfo(string id)
    {
        return m_players.SingleOrDefault(p => p.Id == id);
    }
}
