using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using ShallowNet;

public class Boat : MonoBehaviour
{
    public float m_movementSpeed = 2.5f;
    public float m_rotationSpeed = 720;

    public string PlayerId { get; set; }

	internal Dictionary<FishType, float> m_catch;

    public UnityEngine.UI.Text m_nameLabel;
    public UnityEngine.UI.Text m_tooltipLabel;
    public CanvasGroup m_tooltipGroup;

    public IntVector2 CurrentCell
    {
        get
        {
            Vector3 pos = transform.position;
            return new IntVector2(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
        }
    }

    internal bool isLocalPlayer { get { return PlayerId == MyNetworkManager.Instance.LocalPlayerId; } }

    // Use this for initialization
    void Start()
    {
		m_catch = new Dictionary<FishType, float>();
		foreach (FishType ft in FishType.All)
			m_catch.Add(ft, 0);

        var playerInfo = MyNetworkManager.Instance.getPlayerInfo(PlayerId);
        m_nameLabel.text = playerInfo.Name;
        m_nameLabel.color = Util.HSVToRGB(playerInfo.ColourH, playerInfo.ColourS, playerInfo.ColourV);
    }

    internal void setColour(Color colour)
    {
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            foreach (Material material in renderer.materials)
                material.color = colour;
        }

        m_nameLabel.color = colour;
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

            m_nameLabel.enabled = value;
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
        for (int i = 1; i < m_course.Count; i++)
        {
            len += Vector3.Distance(m_course[i], m_course[i - 1]);
            m_courseSegmentCumulativeLengths.Add(len);
        }

        m_courseEndTime = msg.StartTime + len / m_movementSpeed;

        transform.position = m_course[0];

        if (isLocalPlayer)
        {
            GameManager.Instance.CourseLine.setCourse(m_course);
        }
    }

    #endregion

    #region Casting

    internal string m_castGear = null;
    internal float m_castStartTime, m_castEndTime;

    internal void setCasting(SetPlayerCastingGear msg)
    {
        m_course.Clear();
        if (isLocalPlayer)
            GameManager.Instance.CourseLine.setCourse(m_course);

        transform.position = new Vector3(msg.Position.x, 0, msg.Position.y);

        m_castGear = msg.GearName;
        m_castStartTime = msg.StartTime;
        m_castEndTime = msg.EndTime;
    }

    #endregion

    private string getTooltipText()
    {
        var playerInfo = MyNetworkManager.Instance.getPlayerInfo(PlayerId);
        string tooltipText = string.Format("{0}", playerInfo.Name);

        tooltipText += "\nCurrent catch: ";
        tooltipText += string.Join(", ", (from n in m_catch select n.ToString()).ToArray());

        if (m_castGear != null)
        {
            tooltipText += string.Format("\nCurrently casting {0}", m_castGear);
        }

        return tooltipText;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_castGear != null && GameManager.Instance.CurrentTime > m_castEndTime)
        {
            m_castGear = null;
            m_castStartTime = m_castEndTime = 0;
        }

        Quaternion targetRotation = Quaternion.identity;

        if (m_course.Count > 0)
        {
            float lengthAlongCourse = (GameManager.Instance.CurrentTime - m_courseStartTime) * m_movementSpeed;

            if (lengthAlongCourse <= 0)
            {
                transform.position = m_course[0];
            }
            else if (lengthAlongCourse >= m_courseSegmentCumulativeLengths.Last())
            {
                transform.position = m_course.Last();
            }
            else
            {
                for (int i = 1; i < m_course.Count; i++)
                {
                    if (lengthAlongCourse < m_courseSegmentCumulativeLengths[i])
                    {
                        // Between segments i-1 and i
                        float a = m_courseSegmentCumulativeLengths[i - 1];
                        float b = m_courseSegmentCumulativeLengths[i];
                        float p = (lengthAlongCourse - a) / (b - a);
                        transform.position = Vector3.Lerp(m_course[i - 1], m_course[i], p);
                        targetRotation = Quaternion.FromToRotation(Vector3.right, m_course[i] - m_course[i - 1]);

                        if (isLocalPlayer)
                        {
                            GameManager.Instance.CourseLine.setOffset(i - 1 + p);
                        }

                        break;
                    }
                }
            }
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, m_rotationSpeed * Time.deltaTime);

        if (isLocalPlayer)
        {
            if (GameManager.Instance.getFishDensity(CurrentCell) == null)
            {
                RequestFishDensity msg = new RequestFishDensity() { X = CurrentCell.X, Y = CurrentCell.Y, Width = 1, Height = 1 };
                MyNetworkManager.Instance.m_client.sendMessage(msg);
            }
        }
        else // not local player
        {
            bool visible = GameManager.Instance.m_fogCircle.cellIsVisible(this.CurrentCell);
            setVisible(visible);

            if (visible)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 yIntercept = ray.GetPoint(-ray.origin.y / ray.direction.y);
                yIntercept.y = 0;

                if (Vector3.Distance(yIntercept, transform.position) <= 1)
                {
                    m_tooltipGroup.alpha = 1;
                    m_tooltipLabel.text = getTooltipText();
                }
                else
                {
                    m_tooltipGroup.alpha = 0;
                }
            }
        }
    }
}

