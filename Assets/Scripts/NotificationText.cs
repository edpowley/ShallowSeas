using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class NotificationText : MonoBehaviour {

    private Text m_text;
    public float Duration = 5;
    private float m_lifetime = float.PositiveInfinity;

	// Use this for initialization
	void Start()
    {
        m_text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update()
    {
        if (m_lifetime < Duration)
        {
            Color oldColor = m_text.color;
            m_text.color = new Color(oldColor.r, oldColor.g, oldColor.b, 1.0f - m_lifetime/Duration);
            m_lifetime += Time.deltaTime;
        }
        else
        {
            m_text.text = "";
        }
	}

    internal void PutMessage(string msg)
    {
        m_text.text = msg;
        m_lifetime = 0;
    }

    internal void PutMessage(string fmt, params object[] args)
    {
        PutMessage(string.Format(fmt, args));
    }
}
