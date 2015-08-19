using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Linq;

public class Boat : MonoBehaviour
{
    //private List<Vector3> m_course = new List<Vector3>();
    private bool m_courseBeingDrawn = false;
    public float MovementSpeed = 10;
    public float RotationSpeed = 90;
    public MeshRenderer NetRenderer;
    internal List<int> m_currentCatch = new List<int>{0,0,0};

    internal string m_castGear = null;
    internal float m_castProgress;

    public MyNetworkPlayer Player;

    public IntVector2 CurrentCell
    {
        get
        {
            Vector3 pos = transform.position;
            return new IntVector2(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
        }
    }

    private bool isLocalPlayer { get { return Player != null && Player.isLocalPlayer; } }

    // Use this for initialization
    void Start()
    {
        if (isLocalPlayer)
        {
            GameManager.Instance.m_localPlayerBoat = this;
            GameManager.Instance.CourseLine.setStartPoint(transform.position);
            StartCoroutine(handleMouse());
        }
    }

    internal void setColour(Color colour)
    {
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            foreach (Material material in renderer.materials)
                material.color = colour;
        }
    }

    void OnDestroy()
    {
        try
        {
            if (GameManager.Instance.m_localPlayerBoat == this)
                GameManager.Instance.m_localPlayerBoat = null;
        }
        catch (System.NullReferenceException)
        {
            // do nothing
        }
    }
    
    /*private void addCoursePoint(Vector3 p)
    {
        Player.Course.Add(new CoursePoint(p.x, p.z));
        if (isLocalPlayer)
            GameManager.Instance.CourseLine.addPoint(p);
    }

    private void removeFirstCoursePoint()
    {
        Player.Course.RemoveAt(0);
        if (isLocalPlayer)
            GameManager.Instance.CourseLine.removeFirstPoint();
    }

    private void clearCourse()
    {
        Player.Course.Clear();
        if (isLocalPlayer)
            GameManager.Instance.CourseLine.clearPoints();
    }

    private bool isCourseEmpty()
    {
        return Player.Course.Count == 0;
    }

    private Vector3 getFirstCoursePoint()
    {
        return Player.Course [0].toVector3();
    }
    
    private Vector3 getLastCoursePoint()
    {
        return Player.Course [Player.Course.Count - 1].toVector3();
    }
    
    private Vector3 getLastCoursePointOrDefault(Vector3 def)
    {
        return isCourseEmpty() ? def : getLastCoursePoint();
    }*/
    
    private IEnumerator handleMouse()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                List<Vector3> course = new List<Vector3>();
                course.Add(transform.position);
                m_courseBeingDrawn = true;

                while (Input.GetMouseButton(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Vector3 yIntercept = ray.GetPoint(-ray.origin.y / ray.direction.y);
                    
                    //if (!Mathf.Approximately(yIntercept.y, 0))
                    //    Debug.LogErrorFormat("yIntercept.y == {0} != 0", yIntercept.y);
                    
                    yIntercept.y = 0;
                    
                    //Debug.LogFormat("yIntercept: {0}", yIntercept);

                    if (Vector3.Distance(yIntercept, course.Last()) > 0.5f)
                    {
                        // addCoursePoint(yIntercept);

                        Vector3 pathStart = course.Last();
                        List<Vector3> path = Pathfinder.FindPath(pathStart, yIntercept);
                        if (path != null)
                        {
                            Pathfinder.PullString(path);

                            // First element of path is the start position
                            course.AddRange(path.Skip(1));
                        }
                    }

                    yield return null;
                }

                m_courseBeingDrawn = false;
                /*Player.Course.Clear();
                foreach (Vector3 p in course)
                    Player.Course.Add(new CoursePoint(p.x, p.z));*/

                Player.SetCourse(course);
            }

            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_castGear != null)
        {
            // do nothing (prevent boat from moving whilst gear is cast)
        }
        else if (!m_courseBeingDrawn)
        {
            /*float movementStepSize = MovementSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.LeftShift))
                movementStepSize *= 20;

            while (movementStepSize > 0 && !isCourseEmpty())
            {
                Vector3 target = getFirstCoursePoint();
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

            if (isLocalPlayer)
                GameManager.Instance.CourseLine.setStartPoint(transform.position);*/
        }
    }

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
        
        List<float> density = GameManager.Instance.getFishDensity(CurrentCell);
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
        GameManager.Instance.AddCatch(fishCaught);
    }
}

