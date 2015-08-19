using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Linq;

public class Boat : MonoBehaviour
{
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

    private IEnumerator handleMouse()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Player.ClearCourse();
                List<Vector3> course = new List<Vector3>();
                course.Add(transform.position);

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

                Player.SetCourse(course);
            }

            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion targetRotation = Quaternion.identity;

        if (m_castGear != null)
        {
            // do nothing (prevent boat from moving whilst gear is cast)
        }
        else if (Player.m_course.Count > 0)
        {
            float lengthAlongCourse = (Time.timeSinceLevelLoad - Player.m_courseStartTime) * MovementSpeed;

            if (lengthAlongCourse <= 0)
            {
                transform.position = Player.m_course [0];
            }
            else if (lengthAlongCourse >= Player.m_courseSegmentCumulativeLengths.Last())
            {
                transform.position = Player.m_course.Last();
            }
            else
            {
                for (int i=1; i<Player.m_course.Count; i++)
                {
                    if (lengthAlongCourse < Player.m_courseSegmentCumulativeLengths [i])
                    {
                        // Between segments i-1 and i
                        float a = Player.m_courseSegmentCumulativeLengths [i - 1];
                        float b = Player.m_courseSegmentCumulativeLengths [i];
                        float p = (lengthAlongCourse - a) / (b - a);
                        transform.position = Vector3.Lerp(Player.m_course [i - 1], Player.m_course [i], p);
                        targetRotation = Quaternion.FromToRotation(Vector3.right, Player.m_course[i] - Player.m_course[i-1]);
                        break;
                    }
                }
            }
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
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

