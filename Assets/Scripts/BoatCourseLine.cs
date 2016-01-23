using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using ShallowNet;

public class BoatCourseLine : MonoBehaviour
{
    public GameObject m_linePrefab;
    public bool m_handlesMouse = false;

    private List<GameObject> m_lines = new List<GameObject>();
    private GameObject m_firstLine = null;
    private List<Vector3> m_points = new List<Vector3>();
    private float m_offset = 0;

    void Start()
    {
        if (m_handlesMouse)
            StartCoroutine(handleMouse());
    }

    private IEnumerator handleMouse()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 startPos = GameManager.Instance.LocalPlayerBoat.transform.position;

                {
                    RequestCourse msg = new RequestCourse();
                    msg.Course = new List<SNVector2> { new SNVector2(startPos.x, startPos.z) };
                    MyNetworkManager.Instance.m_client.sendMessage(msg);
                }

                clearPoints();
                addPoint(startPos);

                while (Input.GetMouseButton(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Vector3 yIntercept = ray.GetPoint(-ray.origin.y / ray.direction.y);

                    yIntercept.y = 0;

                    if (Vector3.Distance(yIntercept, m_points.Last()) > 0.5f)
                    {
                        Vector3 pathStart = m_points.Last();
                        List<Vector3> path = Pathfinder.FindPath(pathStart, yIntercept);
                        if (path != null)
                        {
                            Pathfinder.PullString(path);

                            // First element of path is the start position
                            addPoints(path.Skip(1));
                        }
                    }

                    yield return null;
                }

                {
                    RequestCourse msg = new RequestCourse();
                    msg.Course = new List<SNVector2>(from p in m_points select new SNVector2(p.x, p.z));
                    MyNetworkManager.Instance.m_client.sendMessage(msg);

                    HashSet<IntVector2> squaresOnCourse = new HashSet<IntVector2>(getSquaresAlongCourse(m_points));
                    RequestFishDensity densityMsg = new RequestFishDensity()
                    {
                        Squares = new List<SNVector2>(from p in squaresOnCourse select new SNVector2(p.X, p.Y))
                    };
                    MyNetworkManager.Instance.m_client.sendMessage(densityMsg);
                }

                /*if (Player.m_castGear == GearType.None)
                {
                    Player.SetCourse(course);
                }*/

                clearPoints();
            }

            yield return null;
        }
    }

    private static IEnumerable<IntVector2> getSquaresAlongCourse(List<Vector3> points)
    {
        if (points.Count > 0)
        {
            yield return new IntVector2((int)points[0].x, (int)points[0].z);
            for (int i = 1; i < points.Count; i++)
            {
                foreach (IntVector2 square in Util.SupercoverLine(points[i - 1].x, points[i - 1].z, points[i].x, points[i].z))
                    yield return square;
            }
        }
    }

    private void setLineSegmentTransformation(GameObject line, Vector3 a, Vector3 b)
    {
        line.transform.localPosition = a;
        line.transform.localScale = new Vector3((b - a).magnitude, 1, 1);
        line.transform.localRotation = Quaternion.FromToRotation(Vector3.right, b - a);
    }

    internal void addPoint(Vector3 p)
    {
        if (m_offset != 0)
            throw new System.NotImplementedException("Cannot add points when the offset is nonzero");

        if (m_points.Count > 0)
        {
            GameObject newLine = Util.InstantiatePrefab(m_linePrefab);
            newLine.transform.SetParent(this.transform, worldPositionStays: false);
            setLineSegmentTransformation(newLine, m_points[m_points.Count - 1], p);
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
        Debug.Log("clearPoints()");

        foreach (GameObject line in m_lines)
        {
            Destroy(line);
        }

        m_lines.Clear();
        m_points.Clear();
        m_offset = 0;

        if (m_firstLine != null)
        {
            Destroy(m_firstLine);
            m_firstLine = null;
        }
    }

    internal void setOffset(float offset)
    {
        int intOffset = Mathf.CeilToInt(offset);
        float fracOffset = offset - Mathf.Floor(offset);

        for (int i = 0; i < m_lines.Count; i++)
            m_lines[i].SetActive(i >= intOffset);

        if (fracOffset != 0 && intOffset > 0 && intOffset < m_points.Count)
        {
            Vector3 p = m_points[intOffset - 1];
            Vector3 q = m_points[intOffset];
            Vector3 s = Vector3.Lerp(p, q, fracOffset);

            if (m_firstLine == null)
            {
                m_firstLine = Util.InstantiatePrefab(m_linePrefab);
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
