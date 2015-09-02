﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class MyNetworkPlayer : NetworkBehaviour
{
    internal List<Vector3> m_course = new List<Vector3>();
    internal List<float> m_courseSegmentCumulativeLengths = new List<float>();
    internal float m_courseStartTime, m_courseEndTime;

    internal List<int> m_currentCatch = new List<int>{0,0,0};

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

    #region Course

    [ClientRpc]
    public void RpcSetCourse(Vector3[] course, Vector3 pos, float timestamp)
    {
        Debug.LogFormat("RpcSetCourse('{0}', {1}) -- time is now {2}", course, timestamp, Time.timeSinceLevelLoad);

        m_course.Clear();
        m_course.AddRange(course);
        m_courseStartTime = timestamp;

        m_courseSegmentCumulativeLengths.Clear();
        float len = 0;
        m_courseSegmentCumulativeLengths.Add(0);
        for (int i=1; i<m_course.Count; i++)
        {
            len += Vector3.Distance(m_course[i], m_course[i-1]);
            m_courseSegmentCumulativeLengths.Add(len);
        }

        m_courseEndTime = timestamp + len / m_boat.MovementSpeed;

        m_boat.transform.position = pos;

        if (isLocalPlayer)
        {
            GameManager.Instance.CourseLine.setCourse(m_course);
        }
    }

    [Command]
    public void CmdSetCourse(Vector3[] course)
    {
        Debug.LogFormat("CmdSetCourse('{0}')", course);

        RpcSetCourse(course, m_boat.transform.position, Time.timeSinceLevelLoad);
    }

    internal void SetCourse(List<Vector3> course)
    {
        Debug.LogFormat("[Cmd] Set a course of {0} points", course.Count);

        CmdSetCourse(course.ToArray());
    }

    internal void ClearCourse()
    {
        CmdSetCourse(new Vector3[0]);
    }

    #endregion

    #region Gear casting

    internal string m_castGear = null;
    internal float m_castProgress;
    
    internal void CastGear(CastGearButton gear)
    {
        StartCoroutine(castCoroutine(gear));
    }
    
    private IEnumerator castCoroutine(CastGearButton gear)
    {
        m_castGear = gear.GearName;
        m_castProgress = 0;
        float progressPerSecond = 1.0f / gear.CastDuration;
        int totalFishCaught = 0;
        List<int> fishCaught = new List<int>();
        
        List<float> density = GameManager.Instance.getFishDensity(m_boat.CurrentCell);
        for (int i=0; i<density.Count; i++)
        {
            fishCaught.Add(0);
        }
        
        while (m_castProgress < 1.0f)
        {
            m_castProgress += progressPerSecond * Time.deltaTime;
            
            if (totalFishCaught < gear.MaxCatch)
            {
                int fishIndex = Random.Range(0, density.Count);
                
                if (Random.Range(0.0f, 1.0f) < density[fishIndex] * Time.deltaTime * gear.CatchMultiplier[fishIndex])
                {
                    fishCaught[fishIndex]++;
                    totalFishCaught++;
                }
            }
            
            yield return null;
        }
        
        m_castGear = null;
        AddCatch(fishCaught);
    }

    public void AddCatch(List<int> fishCaught)
    {
        List<string> notificationStrings = new List<string>();

        for (int i=0; i<fishCaught.Count; i++)
        {
            m_currentCatch[i] += fishCaught[i];
            if (fishCaught[i] > 0)
                notificationStrings.Add(string.Format("{0} {1}", fishCaught[i], GameManager.Instance.FishNames[i]));
        }

        switch (notificationStrings.Count)
        {
            case 0:
                GameManager.Instance.Notification.PutMessage("You caught nothing!");
                break;

            case 1:
                GameManager.Instance.Notification.PutMessage("You caught {0}", notificationStrings[0]);
                break;

            default:
                GameManager.Instance.Notification.PutMessage("You caught {0} and {1}",
                                        string.Join(", ", notificationStrings.Take(notificationStrings.Count-1).ToArray()),
                                        notificationStrings.Last()
                                        );
                break;
        }
    }

    #endregion

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
        if (isServer)
        {
            if (m_course.Count > 0 && Time.timeSinceLevelLoad >= m_courseEndTime)
            {
                RpcSetCourse(new Vector3[0], m_course.Last(), Time.timeSinceLevelLoad);
            }
        }

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
