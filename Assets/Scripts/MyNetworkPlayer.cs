using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class MyNetworkPlayer : NetworkBehaviour
{
    public static MyNetworkPlayer LocalInstance { get; private set; }
    private static List<MyNetworkPlayer> s_instances = new List<MyNetworkPlayer>();

    public static IEnumerable<MyNetworkPlayer> Instances { get { return s_instances; } }

    public Boat PlayerBoatPrefab;

    internal Boat m_boat;

    [SyncVar]
    public string PlayerName;

    [SyncVar]
    public Color PlayerColour;

    [Command]
    public void CmdSetName(string name)
    {
        if (name == "")
            PlayerName = string.Format("Player {0}", s_instances.IndexOf(this) + 1);
        else
            PlayerName = name;

        gameObject.name = "MyNetworkPlayer - " + PlayerName;
    }

    public void SetName(string name)
    {
        if (name != PlayerName)
            CmdSetName(name);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (LocalInstance != null)
            Debug.LogError("Multiple local player instances");
        else
            LocalInstance = this;
    }

    public void Awake()
    {
        s_instances.Add(this);
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
        OnLevelWasLoaded(Application.loadedLevel);
    }

    public void Update()
    {
    }

    public void OnLevelWasLoaded(int levelIndex)
    {
        Level level = (Level)levelIndex;

        if (isLocalPlayer)
        {
            if (!ClientScene.ready)
            {
                Debug.Log("Setting ready");
                ClientScene.Ready(connectionToServer);
            }
            
            //ClientScene.ClearSpawners();
            //ClientScene.RegisterPrefab(MyNetworkManager.Instance.BoatPrefab);
        }
        
        if (isClient)
        {
            Debug.LogFormat("OnLevelWasLoaded: client {0}", playerControllerId);

            //PlayerBoat.gameObject.SetActive(level == Level.MainGame);

            if (level == Level.MainGame)
            {
                m_boat = Util.InstantiatePrefab(PlayerBoatPrefab);
                m_boat.Player = this;
                m_boat.setColour(PlayerColour);
            }
            else
            {
                m_boat = null;
            }
        }

        if (isServer)
        {
            Debug.Log("OnLevelWasLoaded: server");

            /*switch ((Level)level)
            {
                case Level.MainGame:
                    createBoat();
                    break;
            }*/
        }
    }

    public void OnDestroy()
    {
        if (LocalInstance == this)
            LocalInstance = null;

        s_instances.Remove(this);
    }

    public static void updateColours()
    {
        for (int i=0; i<s_instances.Count; i++)
        {
            float hue = (float)i / (float)s_instances.Count;

            s_instances[i].PlayerColour = Util.HSVToRGB(hue, 0.7f, 1.0f);
        }
    }

    private void createBoat()
    {
        Boat boat = Util.InstantiatePrefab(PlayerBoatPrefab);
        NetworkServer.Spawn(boat.gameObject);
    }
}
