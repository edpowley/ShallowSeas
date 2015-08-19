using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MyNetworkManager : NetworkManager
{
    //public static MyNetworkManager Instance { get { return FindObjectOfType<MyNetworkManager>(); } }
    public static MyNetworkManager Instance { get; private set; }

    public GameObject BoatPrefab;

    public bool isClient { get; private set; }
    public bool isServer { get; private set; }

    public void Awake()
    {
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

        isClient = isServer = false;

        DontDestroyOnLoad(this.gameObject);
    }
    
    public void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public override void OnStartClient(NetworkClient client)
    {
        Debug.Log("OnStartClient");
        isClient = true;
        //ClientScene.RegisterPrefab(BoatPrefab);
        base.OnStartClient(client);
    }

    public override void OnStartServer()
    {
        Debug.Log("OnStartServer");
        isServer = true;
        base.OnStartServer();

        //StartCoroutine(foo());
    }

    private IEnumerator foo()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            Debug.Log("Spawning a boat");
            GameObject ob = (GameObject)Instantiate(BoatPrefab, UnityEngine.Random.insideUnitSphere * 5, UnityEngine.Random.rotationUniform);
            NetworkServer.Spawn(ob);
        }
    }

    public override void OnStopClient()
    {
        Debug.Log("OnStopClient");
        base.OnStopClient();
        isClient = false;
    }

    public override void OnStopServer()
    {
        Debug.Log("OnStopServer");
        base.OnStopServer();
        isServer = false;
    }

    public void Stop()
    {
        if (isClient)
            StopClient();

        if (isServer)
            StopServer();
    }

    public void ChangeLevel(Level level)
    {
        ServerChangeScene(level.ToString());
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("OnServerAddPlayer");
        base.OnServerAddPlayer(conn, playerControllerId);
        MyNetworkPlayer.updateColours();
    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        Debug.Log("OnServerRemovePlayer");
        base.OnServerRemovePlayer(conn, player);
        MyNetworkPlayer.updateColours();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnServerDisconnect");
        base.OnServerDisconnect(conn);
        MyNetworkPlayer.updateColours();
    }

    public void Update()
    {
    }
}
