using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public static class Pathfinder
{

    private class OpenSetElement
    {
        public OpenSetElement(IntVector2 v, float g, float h, OpenSetElement cameFrom)
        {
            m_v = v;
            m_g = g;
            m_h = h;
            m_cameFrom = cameFrom;
        }

        public readonly IntVector2 m_v;
        public readonly float m_g, m_h;
        public readonly OpenSetElement m_cameFrom;
        public float F { get { return m_g + m_h; } }
    }

    private class OpenQueue
    {
        private List<OpenSetElement> m_list;

        public OpenQueue()
        {
            m_list = new List<OpenSetElement>();
        }

        public bool IsEmpty { get { return m_list.Count == 0; } }

        public void Add(OpenSetElement e)
        {
            int index;
            float f = e.F;
            for (index = m_list.Count; index > 0 && m_list[index-1].F < f; index--)
            {
                // do nothing
            }

            m_list.Insert(index, e);
        }

        public OpenSetElement Pop()
        {
            OpenSetElement result = m_list[m_list.Count - 1];
            m_list.RemoveAt(m_list.Count - 1);
            return result;
        }

        public IEnumerable<OpenSetElement> Elements{ get { return m_list; } }
    }

    public static List<Vector3> FindPath(Vector3 start, Vector3 end)
    {
        GameManager gm = GameManager.Instance;

        IntVector2 startSquare = new IntVector2(Mathf.FloorToInt(start.x), Mathf.FloorToInt(start.z));
        IntVector2 endSquare = new IntVector2(Mathf.FloorToInt(end.x), Mathf.FloorToInt(end.z));

        if (!gm.isWater(startSquare) || !gm.isWater(endSquare))
        {
            return null;
        }

        OpenQueue openSet = new OpenQueue();
        openSet.Add(new OpenSetElement(startSquare, 0, (endSquare-startSquare).Magnitude, null));
        HashSet<IntVector2> closedSet = new HashSet<IntVector2>();
        closedSet.Add(startSquare);

        while (!openSet.IsEmpty)
        {
            /*yield return null;

            foreach(var v in openSet.Elements)
            {
                IntVector2 q = v.m_v;
                for (var u=v.m_cameFrom; u!=null; u=u.m_cameFrom)
                {
                    IntVector2 p = u.m_v;
                    Debug.DrawLine(new Vector3(p.X,0,p.Y), new Vector3(q.X,0,q.Y), Color.yellow);
                    q=p;
                }
            }

            foreach(var v in closedSet)
            {
                Debug.DrawLine(new Vector3(v.X-0.1f, 0, v.Y-0.1f), new Vector3(v.X+0.1f, 0, v.Y+0.1f), Color.red);
                Debug.DrawLine(new Vector3(v.X-0.1f, 0, v.Y+0.1f), new Vector3(v.X+0.1f, 0, v.Y-0.1f), Color.red);
            }*/
            
            OpenSetElement front = openSet.Pop();

            if (front.m_v.Equals(endSquare))
            {
                Debug.LogFormat("Closed set size = {0}", closedSet.Count);
                return reconstructPath(start, end, front);
            }

            for (int dx=-1; dx<=1; dx++)
            {
                for (int dy=-1; dy<=1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    if (dx != 0 && dy != 0)
                        continue;

                    IntVector2 d = new IntVector2(dx, dy);
                    IntVector2 neighbour = front.m_v + d;

                    if (gm.isWater(neighbour) && !closedSet.Contains(neighbour))
                    {
                        openSet.Add(new OpenSetElement(neighbour, front.m_g + d.Magnitude, (endSquare-neighbour).Magnitude, front));
                        closedSet.Add(neighbour);
                    }
                }
            }
        }

        return null;
    }

    private static List<Vector3> reconstructPath(Vector3 start, Vector3 end, OpenSetElement e)
    {
        List<Vector3> result = new List<Vector3>();
        result.Add(end);
        
        for (; e != null; e = e.m_cameFrom)
        {
            result.Add(new Vector3(e.m_v.X + 0.5f, 0, e.m_v.Y + 0.5f));
        }
        
        result.Add(start);
        
        result.Reverse();
        
        return result;
    }

    public static void PullString(List<Vector3> path)
    {
        GameManager gm = GameManager.Instance;

        for (int i = 1; i < path.Count - 1; i++)
        {
            if (straightLinePathIsClear(gm, path[i-1], path[i+1]))
            {
                path.RemoveAt(i);
                i--;
            }
        }
    }

    private static bool straightLinePathIsClear(GameManager gm, Vector3 a, Vector3 b)
    {
        foreach (IntVector2 s in Util.SupercoverLine(a.x, a.z, b.x, b.z))
        {
            if (!gm.isWater(s))
                return false;
        }

        return true;
    }
}

