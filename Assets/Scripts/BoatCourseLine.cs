using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoatCourseLine : MonoBehaviour
{
    public GameObject LinePrefab;

    private List<GameObject> m_lines = new List<GameObject>();
    private List<Vector3> m_points = new List<Vector3>();

    private void setLineSegmentTransformation(GameObject line, Vector3 a, Vector3 b)
    {
        line.transform.localPosition = a;
        line.transform.localScale = new Vector3((b-a).magnitude, 1, 1);
        line.transform.localRotation = Quaternion.FromToRotation(Vector3.right, b-a);
    }

    internal void addPoint(Vector3 p)
    {
        GameObject newLine = Util.InstantiatePrefab(LinePrefab);
        newLine.transform.SetParent(this.transform, worldPositionStays: false);
        setLineSegmentTransformation(newLine, m_points[m_points.Count - 1], p);
        m_lines.Add(newLine);
        m_points.Add(p);
    }

    internal void removeFirstPoint()
    {
        Destroy(m_lines[0]);
        m_lines.RemoveAt(0);
        m_points.RemoveAt(1);
    }

    internal void clearPoints()
    {
        foreach (GameObject line in m_lines)
        {
            Destroy(line);
        }

        m_lines.Clear();
        m_points.RemoveRange(1, m_points.Count-1);
    }

    internal void setStartPoint(Vector3 p)
    {
        if (m_points.Count == 0)
            m_points.Add(p);
        else
            m_points[0] = p;

        if (m_lines.Count > 0)
        {
            setLineSegmentTransformation(m_lines[0], m_points[0], m_points[1]);
        }
    }

    // Update is called once per frame
    void Update()
    {
    
    }
}
