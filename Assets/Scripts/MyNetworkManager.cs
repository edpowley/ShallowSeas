using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using ShallowNet;

public class MyNetworkManager : MonoBehaviour
{
    public static MyNetworkManager Instance { get; private set; }

    internal ClientWrapper m_client = null;
    internal string m_localPlayerId;

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
}
