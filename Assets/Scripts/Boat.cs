﻿using UnityEngine;
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

    public MyNetworkPlayer Player;

    public UnityEngine.UI.Text NameLabel;

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
            StartCoroutine(handleMouse());
        }

        NameLabel.text = Player.PlayerName;
        NameLabel.color = Player.PlayerColour;
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

                BoatCourseLine courseLine = GameManager.Instance.DrawingLine;
                courseLine.clearPoints();
                courseLine.addPoint(transform.position);
                
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
                            courseLine.addPoints(path.Skip(1));
                        }
                    }

                    yield return null;
                }

                courseLine.clearPoints();
                Player.SetCourse(course);
            }

            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion targetRotation = Quaternion.identity;

        if (Player.m_castGear != null)
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

                        if (isLocalPlayer)
                        {
                            GameManager.Instance.CourseLine.setOffset(i-1 + p);
                        }

                        break;
                    }
                }
            }
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
    }
}

