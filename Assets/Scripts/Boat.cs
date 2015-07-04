using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Boat : MonoBehaviour
{
    private List<Vector3> m_course = new List<Vector3>();
    private bool m_courseBeingDrawn = false;
    public float MovementSpeed = 10;
    public float RotationSpeed = 90;
    public BoatCourseLine CourseLine;
    internal GearItem m_currentCastGear = null;
    public MeshRenderer NetRenderer;
    internal List<int> m_currentCatch = new List<int>{0,0,0};

    public IntVector2 CurrentCell
    {
        get
        {
            Vector3 pos = transform.position;
            return new IntVector2(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
        }
    }

    // Use this for initialization
    void Start()
    {
        CourseLine.setStartPoint(transform.position);
        StartCoroutine(handleMouse());
    }

    private void addCoursePoint(Vector3 p)
    {
        m_course.Add(p);
        CourseLine.addPoint(p);
    }

    private void removeFirstCoursePoint()
    {
        m_course.RemoveAt(0);
        CourseLine.removeFirstPoint();
    }

    private void clearCourse()
    {
        m_course.Clear();
        CourseLine.clearPoints();
    }

    private IEnumerator handleMouse()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                clearCourse();
                m_courseBeingDrawn = true;

                while (Input.GetMouseButton(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Vector3 yIntercept = ray.GetPoint(-ray.origin.y / ray.direction.y);
                    
                    //if (!Mathf.Approximately(yIntercept.y, 0))
                    //    Debug.LogErrorFormat("yIntercept.y == {0} != 0", yIntercept.y);
                    
                    yIntercept.y = 0;
                    
                    //Debug.LogFormat("yIntercept: {0}", yIntercept);

                    if (m_course.Count == 0 || Vector3.Distance(yIntercept, m_course[m_course.Count - 1]) > 0.5f)
                    {
                        addCoursePoint(yIntercept);

                        /*Vector3 pathStart = (m_course.Count > 0) ? m_course[m_course.Count - 1] : transform.position;
                        var path = Pathfinder.FindPath(pathStart, yIntercept);

                        if (path != null)
                        {
                            bool first = true;
                            foreach (Vector3 p in path)
                            {
                                if (!first)
                                    addCoursePoint(p);

                                first = false;
                            }
                        }*/
                    }

                    yield return null;
                }

                m_courseBeingDrawn = false;
            }

            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_currentCastGear != null)
        {
            if (!m_currentCastGear.IsCast)
            {
                m_currentCastGear = null;
            }
        }
        else if (!m_courseBeingDrawn)
        {
            float movementStepSize = MovementSpeed * Time.deltaTime;

            while (movementStepSize > 0 && m_course.Count > 0)
            {
                Vector3 target = m_course [0];
                Vector3 delta = target - transform.position;

                float deltaSize = delta.magnitude;
                if (deltaSize < movementStepSize)
                {
                    transform.position = target;
                    movementStepSize -= deltaSize;
                    removeFirstCoursePoint();
                }
                else
                {
                    transform.position += delta / deltaSize * movementStepSize;
                    movementStepSize = 0;
                }

                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.right, delta);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
            }

            CourseLine.setStartPoint(transform.position);
        }
    }

    internal void CastGear(GearItem gear)
    {
        m_currentCastGear = gear;
        gear.Cast();
    }
}
