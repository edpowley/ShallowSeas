using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;

public struct CoursePoint
{
    public float x, y;

    public CoursePoint(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector3 toVector3()
    {
        return new Vector3(x, 0, y);
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", x, y);
    }
}

public class SyncListCoursePoint : SyncListStruct<CoursePoint> {}

public class MyNetworkPlayer : NetworkBehaviour
{
    //public SyncListCoursePoint Course = new SyncListCoursePoint();
    internal List<Vector3> m_course = new List<Vector3>();

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

    private string serializeCourse(List<Vector3> course)
    {
        string[] strings = new string[course.Count];
        for (int i=0; i<course.Count; i++)
        {
            strings[i] = string.Format("{0},{1}", course[i].x, course[i].z);
        }

        return string.Join(";", strings);
    }

    private IEnumerable<Vector3> deserializeCourse(string str)
    {
        foreach (string p in str.Split(';'))
        {
            string[] q = p.Split(',');
            float x = float.Parse(q [0]);
            float z = float.Parse(q [1]);
            yield return new Vector3(x, 0, z);
        }
    }

    [ClientRpc]
    public void RpcSetCourse(string course)
    {
        Debug.LogFormat("RpcSetCourse('{0}')", course);

        m_course.Clear();
        m_course.AddRange(deserializeCourse(course));
    }

    [Command]
    public void CmdSetCourse(string course)
    {
        Debug.LogFormat("CmdSetCourse('{0}')", course);

        RpcSetCourse(course);
    }

    internal void SetCourse(List<Vector3> course)
    {
        Debug.LogFormat("[Cmd] Set a course of {0} points", course.Count);
        CmdSetCourse(serializeCourse(course));
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
        if (m_course.Count > 0)
        {
            Vector3 q = m_course[0];

            for (int i=1; i<m_course.Count; i++)
            {
                Vector3 p = m_course[i];
                Debug.DrawLine(q, p, PlayerColour, 0, false);
                q = p;
            }
        }
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
