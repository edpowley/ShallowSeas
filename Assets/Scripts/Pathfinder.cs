using System;
using UnityEngine;
using System.Collections.Generic;

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
    }

    public static List<Vector3> FindPath(Vector3 start, Vector3 end)
    {
        GameManager gm = GameManager.Instance;

        IntVector2 startSquare = new IntVector2(Mathf.FloorToInt(start.x), Mathf.FloorToInt(start.z));
        IntVector2 endSquare = new IntVector2(Mathf.FloorToInt(end.x), Mathf.FloorToInt(end.z));

        if (!gm.isWater(startSquare) || !gm.isWater(endSquare))
            return null;

        OpenQueue openSet = new OpenQueue();
        openSet.Add(new OpenSetElement(startSquare, 0, (endSquare-startSquare).Magnitude, null));
        HashSet<IntVector2> closedSet = new HashSet<IntVector2>();

        while (!openSet.IsEmpty)
        {
            OpenSetElement front = openSet.Pop();
            closedSet.Add(front.m_v);

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

                    IntVector2 d = new IntVector2(dx, dy);
                    IntVector2 neighbour = front.m_v + d;

                    if (gm.isWater(neighbour) && !closedSet.Contains(neighbour))
                    {
                        openSet.Add(new OpenSetElement(neighbour, front.m_g + d.Magnitude, (endSquare-neighbour).Magnitude, front));
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

}

