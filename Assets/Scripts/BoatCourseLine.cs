using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoatCourseLine : MonoBehaviour
{
    public GameObject LinePrefab;

    private List<GameObject> m_lines = new List<GameObject>();
    private GameObject m_firstLine = null;
    private List<Vector3> m_points = new List<Vector3>();
    private float m_offset = 0;

    private void setLineSegmentTransformation(GameObject line, Vector3 a, Vector3 b)
    {
        line.transform.localPosition = a;
        line.transform.localScale = new Vector3((b-a).magnitude, 1, 1);
        line.transform.localRotation = Quaternion.FromToRotation(Vector3.right, b-a);
    }

    internal void addPoint(Vector3 p)
    {
        if (m_offset != 0)
            throw new System.NotImplementedException("Cannot add points when the offset is nonzero");

        if (m_points.Count > 0)
        {
            GameObject newLine = Util.InstantiatePrefab(LinePrefab);
            newLine.transform.SetParent(this.transform, worldPositionStays: false);
            setLineSegmentTransformation(newLine, m_points [m_points.Count - 1], p);
            newLine.name = string.Format("m_lines[{0}]", m_lines.Count);
            m_lines.Add(newLine);
        }
        m_points.Add(p);
    }

    internal void addPoints(IEnumerable<Vector3> points)
    {
        foreach (Vector3 p in points)
            addPoint(p);
    }

    internal void clearPoints()
    {
        foreach (GameObject line in m_lines)
        {
            Destroy(line);
        }

        m_lines.Clear();
        m_points.Clear();
        m_offset = 0;
    }

    internal void setOffset(float offset)
    {
        int intOffset = Mathf.CeilToInt(offset);
        float fracOffset = offset - Mathf.Floor(offset);

        for (int i=0; i<m_lines.Count; i++)
            m_lines[i].SetActive(i >= intOffset);

        if (fracOffset != 0 && intOffset > 0 && intOffset < m_points.Count)
        {
            Vector3 p = m_points [intOffset - 1];
            Vector3 q = m_points [intOffset];
            Vector3 s = Vector3.Lerp(p, q, fracOffset);

            if (m_firstLine == null)
            {
                m_firstLine = Util.InstantiatePrefab(LinePrefab);
                m_firstLine.transform.SetParent(this.transform, worldPositionStays: false);
                m_firstLine.name = "m_firstLine";
            }

            m_firstLine.SetActive(true);
            setLineSegmentTransformation(m_firstLine, s, q);
        }
        else
        {
            if (m_firstLine != null)
                m_firstLine.SetActive(false);
        }
    }

    internal void setCourse(IEnumerable<Vector3> points)
    {
        clearPoints();
        addPoints(points);
    }
}
