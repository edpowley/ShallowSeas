using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Linq;
using ShallowNet;

public class Boat : MonoBehaviour
{
    public float MovementSpeed = 10;
    public float RotationSpeed = 90;
    public MeshRenderer NetRenderer;

    public string PlayerId { get; set; }

    public UnityEngine.UI.Text NameLabel;

    public IntVector2 CurrentCell
    {
        get
        {
            Vector3 pos = transform.position;
            return new IntVector2(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
        }
    }

    private bool isLocalPlayer { get { return PlayerId == MyNetworkManager.Instance.LocalPlayerId; } }

    // Use this for initialization
    void Start()
    {
        var playerInfo = MyNetworkManager.Instance.getPlayerInfo(PlayerId);
        NameLabel.text = playerInfo.Name;
        NameLabel.color = Util.HSVToRGB(playerInfo.ColourH, playerInfo.ColourS, playerInfo.ColourV);
    }

    internal void setColour(Color colour)
    {
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            foreach (Material material in renderer.materials)
                material.color = colour;
        }

        NameLabel.color = colour;
    }

    private bool m_isVisible = true;

    internal void setVisible(bool value)
    {
        if (value != m_isVisible)
        {
            foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = value;
            }
            
            NameLabel.enabled = value;
        }

        m_isVisible = value;
    }

    void OnDestroy()
    {
    }

    #region Course
    
    internal List<Vector3> m_course = new List<Vector3>();
    internal List<float> m_courseSegmentCumulativeLengths = new List<float>();
    internal float m_courseStartTime, m_courseEndTime;

    internal void setCourse(SetCourse msg)
    {
        m_course.Clear();
        m_course.AddRange(from p in msg.Course select new Vector3(p.x, 0, p.y));
        m_courseStartTime = msg.StartTime;
        
        m_courseSegmentCumulativeLengths.Clear();
        float len = 0;
        m_courseSegmentCumulativeLengths.Add(0);
        for (int i=1; i<m_course.Count; i++)
        {
            len += Vector3.Distance(m_course[i], m_course[i-1]);
            m_courseSegmentCumulativeLengths.Add(len);
        }
        
        m_courseEndTime = msg.StartTime + len / MovementSpeed;
        
        transform.position = m_course[0];
        
        if (isLocalPlayer)
        {
            GameManager.Instance.CourseLine.setCourse(m_course);
        }
    }
    
    #endregion

    // Update is called once per frame
    void Update()
    {
        Quaternion targetRotation = Quaternion.identity;

        if (false) // Player.m_castGear != GearType.None)
        {
            // do nothing (prevent boat from moving whilst gear is cast)
        } else if (m_course.Count > 0)
        {
            float lengthAlongCourse = (GameManager.Instance.CurrentTime - m_courseStartTime) * MovementSpeed;

            if (lengthAlongCourse <= 0)
            {
                transform.position = m_course [0];
            } else if (lengthAlongCourse >= m_courseSegmentCumulativeLengths.Last())
            {
                transform.position = m_course.Last();
            } else
            {
                for (int i=1; i<m_course.Count; i++)
                {
                    if (lengthAlongCourse < m_courseSegmentCumulativeLengths [i])
                    {
                        // Between segments i-1 and i
                        float a = m_courseSegmentCumulativeLengths [i - 1];
                        float b = m_courseSegmentCumulativeLengths [i];
                        float p = (lengthAlongCourse - a) / (b - a);
                        transform.position = Vector3.Lerp(m_course [i - 1], m_course [i], p);
                        targetRotation = Quaternion.FromToRotation(Vector3.right, m_course [i] - m_course [i - 1]);

                        if (isLocalPlayer)
                        {
                            GameManager.Instance.CourseLine.setOffset(i - 1 + p);
                        }

                        break;
                    }
                }
            }
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

        if (!isLocalPlayer)
        {
            setVisible(GameManager.Instance.m_fogCircle.cellIsVisible(this.CurrentCell));
        }
    }
}

