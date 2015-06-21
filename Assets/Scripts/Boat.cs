using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boat : MonoBehaviour
{
    private List<Vector3> m_course = new List<Vector3>();
    private bool m_courseBeingDrawn = false;

    public float MovementSpeed = 10;
    public float RotationSpeed = 90;

    public MeshFilter CourseLineMeshFilter;
    private Mesh m_courseLineMesh;

    private LineMesher m_courseLineMesher = new LineMesher();

    // Use this for initialization
    void Start()
    {
        m_courseLineMesh = new Mesh();
        CourseLineMeshFilter.mesh = m_courseLineMesh;

        StartCoroutine(handleMouse());
    }

    private IEnumerator handleMouse()
    {
        while (true)
        {
            while (!Input.GetMouseButtonDown(0))
            {
                yield return null;
            }

            // Mouse button is down
            m_course.Clear();
            m_courseBeingDrawn = true;

            while (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 yIntercept = ray.GetPoint(-ray.origin.y / ray.direction.y);
                
                if (!Mathf.Approximately(yIntercept.y, 0))
                    Debug.LogErrorFormat("yIntercept.y == {0} != 0", yIntercept.y);
                
                yIntercept.y = 0;
                
                Debug.LogFormat("yIntercept: {0}", yIntercept);

                if (m_course.Count == 0 || Vector3.Distance(yIntercept, m_course[m_course.Count - 1]) > 0.5f)
                {
                    m_course.Add(yIntercept);
                }

                yield return null;
            }

            m_courseBeingDrawn = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_courseBeingDrawn)
        {
            float movementStepSize = MovementSpeed * Time.deltaTime;

            while (movementStepSize > 0 && m_course.Count > 0)
            {
                Vector3 target = m_course[0];
                Vector3 delta = target - transform.position;

                float deltaSize = delta.magnitude;
                if (deltaSize < movementStepSize)
                {
                    transform.position = target;
                    movementStepSize -= deltaSize;
                    m_course.RemoveAt(0);
                }
                else
                {
                    transform.position += delta / deltaSize * movementStepSize;
                    movementStepSize = 0;
                }

                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.right, delta);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
            }
        }

        if (m_course.Count > 0)
        {
            m_courseLineMesher.Clear();
            List<Vector2> course = new List<Vector2>();
            course.Add(new Vector2(transform.position.x, transform.position.z));
            foreach(Vector3 pos in m_course)
            {
                course.Add(new Vector2(pos.x, pos.z));
            }

            m_courseLineMesher.c_radius = 0.25f;
            m_courseLineMesher.AddLineStripAsSegmentsByAngle(course);
            m_courseLineMesher.PopulateMesh(m_courseLineMesh);
        }
        else
        {
            m_courseLineMesh.Clear();
        }
    }
}
