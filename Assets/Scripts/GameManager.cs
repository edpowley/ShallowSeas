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

    public BoatCourseLine CourseLine, DrawingLine;

    public Text TestText, TestText2;
    public NotificationText Notification;

    internal bool isWater(int x, int y)
    {
        if (x < 0 || x >= c_gridWidth || y < 0 || y >= c_gridHeight)
            return false;
        else
            return m_isWater[x,y];
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
            Application.LoadLevel((int)Level.MainMenu);
    }

    public void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Start()
    {
        initIsWater();
        initFishDensity();
    }

    private void initFishDensity()
    {
        for (int x=0; x<c_gridWidth; x++)
        {
            for (int y=0; y<c_gridHeight; y++)
            {
                List<float> density = new List<float>();
                Color pixel = FishDensityMap.GetPixel(x, y);
                density.Add(pixel.r);
                density.Add(pixel.g);
                density.Add(pixel.b);
                m_fishDensity[x, y] = density;
            }
        }
    }

    private void initIsWater()
    {
        Terrain terrain = Terrain.activeTerrain;
        
        for (int x=0; x<c_gridWidth; x++)
        {
            for (int y=0; y<c_gridHeight; y++)
            {
                bool isWater = true;
                
                for (float dx = 0.0f; dx <= 1.0f; dx += 0.5f)
                    for (float dy = 0.0f; dy <= 1.0f; dy += 0.5f)
                        isWater = isWater && (terrain.SampleHeight(new Vector3(x + dx, 0, y + dy)) < 16.0f);
                
                m_isWater[x,y] = isWater;
                
                List<float> density = new List<float>();
                Color pixel = FishDensityMap.GetPixel(x, y);
                density.Add(pixel.r);
                density.Add(pixel.g);
                density.Add(pixel.b);
                m_fishDensity[x, y] = density;
            }
        }

        bool[,] reachable = getReachability(c_gridWidth/2, c_gridHeight/2);

        for (int x=0; x<c_gridWidth; x++)
        {
            for (int y=0; y<c_gridHeight; y++)
            {
                m_isWater[x,y] = m_isWater[x,y] && reachable[x,y];
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

            if (x > 0 && m_isWater[x-1, y] && !result[x-1, y])
                stack.Push(new Vector2(x-1, y));

            if (y > 0 && m_isWater[x, y-1] && !result[x, y-1])
                stack.Push(new Vector2(x, y-1));

            if (x < c_gridWidth-1 && m_isWater[x+1, y] && !result[x+1, y])
                stack.Push(new Vector2(x+1, y));

            if (y < c_gridHeight-1 && m_isWater[x, y+1] && !result[x, y+1])
                stack.Push(new Vector2(x, y+1));
        }

        return result;
    }

    internal List<float> getFishDensity(IntVector2 cell)
    {
        return m_fishDensity[cell.X, cell.Y];
    }

    public void Update()
    {
        IntVector2 currentCell = MyNetworkPlayer.LocalInstance.m_boat.CurrentCell;

        var currentCellFishDensity = m_fishDensity[currentCell.X, currentCell.Y];
        TestText.text = string.Format("Boat in square {0}\nFish density {1}",
                                      currentCell,
                                      string.Join(", ", (from d in currentCellFishDensity select string.Format("{0:0.00}", d)).ToArray())
                                      );

        TestText2.text = string.Format("Catch: {0}",
                                       string.Join(", ", (from n in MyNetworkPlayer.LocalInstance.m_currentCatch select n.ToString()).ToArray())
                                       );
    }
}
