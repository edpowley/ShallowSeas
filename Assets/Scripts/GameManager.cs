using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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

    public Boat PlayerBoat;
    public Text TestText, TestText2;
    public NotificationText Notification;

    internal int CurrentCellX { get; private set; }
    internal int CurrentCellY { get; private set; }
    internal List<float> CurrentCellFishDensity { get; private set; }

    internal bool isWater(int x, int y)
    {
        if (x < 0 || x >= c_gridWidth || y < 0 || y >= c_gridHeight)
            return false;
        else
            return m_isWater[x,y];
    }

    public void Awake()
    {
        if (Instance != null)
            throw new InvalidOperationException("GameManager should be a singleton");

        Instance = this;
    }

    public void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Start()
    {
        Terrain terrain = Terrain.activeTerrain;
        
        for (int x=0; x<c_gridWidth; x++)
        {
            for (int y=0; y<c_gridHeight; y++)
            {
                float height = terrain.SampleHeight(new Vector3(x - c_gridWidth*0.5f + 0.5f, 0, y - c_gridHeight*0.5f + 0.5f));
                m_isWater[x,y] = (height < 16.0f);

                List<float> density = new List<float>();
                Color pixel = FishDensityMap.GetPixel(x, y);
                density.Add(pixel.r);
                density.Add(pixel.g);
                density.Add(pixel.b);
                m_fishDensity[x, y] = density;
            }
        }
    }
    
    public void Update()
    {
        Vector3 boatPos = PlayerBoat.transform.position;
        CurrentCellX = Mathf.FloorToInt(boatPos.x + c_gridWidth*0.5f);
        CurrentCellY = Mathf.FloorToInt(boatPos.z + c_gridHeight*0.5f);

        CurrentCellFishDensity = m_fishDensity[CurrentCellX, CurrentCellY];
        TestText.text = string.Format("Boat in square {0},{1}\nFish density {2}",
                                      CurrentCellX, CurrentCellY,
                                      string.Join(", ", (from d in CurrentCellFishDensity select string.Format("{0:0.00}", d)).ToArray())
                                      );

        TestText2.text = string.Format("Catch: {0}",
                                       string.Join(", ", (from n in PlayerBoat.m_currentCatch select n.ToString()).ToArray())
                                       );
    }

    public void AddCatch(List<int> fishCaught)
    {
        List<string> notificationStrings = new List<string>();

        for (int i=0; i<fishCaught.Count; i++)
        {
            PlayerBoat.m_currentCatch[i] += fishCaught[i];
            if (fishCaught[i] > 0)
                notificationStrings.Add(string.Format("{0} {1}", fishCaught[i], FishNames[i]));
        }

        switch (notificationStrings.Count)
        {
            case 0:
                Notification.PutMessage("You caught nothing!");
                break;

            case 1:
                Notification.PutMessage("You caught {0}", notificationStrings[0]);
                break;

            default:
                Notification.PutMessage("You caught {0} and {1}",
                                        string.Join(", ", notificationStrings.Take(notificationStrings.Count-1).ToArray()),
                                        notificationStrings.Last()
                                        );
                break;
        }
    }
}
