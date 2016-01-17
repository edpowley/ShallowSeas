using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class NotificationText : MonoBehaviour
{
    public Text m_text;
    private CanvasGroup m_canvasGroup;
    public float m_duration = 5;
    private float m_lifetime = float.PositiveInfinity;
    private System.Action m_announceCallback;

    void Start()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (m_lifetime < m_duration)
        {
            m_canvasGroup.interactable = true;
            m_canvasGroup.alpha = 1.0f - m_lifetime / m_duration;
            m_lifetime += Time.deltaTime;
        }
        else
        {
            m_canvasGroup.interactable = false;
            m_canvasGroup.alpha = 0;
            m_announceCallback = null;
        }
    }

    internal void PutMessage(System.Action announceCallback, string msg)
    {
        m_text.text = msg;
        m_lifetime = 0;
        m_announceCallback = announceCallback;
    }

    internal void PutMessage(System.Action announceCallback, string fmt, params object[] args)
    {
        PutMessage(announceCallback, string.Format(fmt, args));
    }

    public void OnAnnounceButton()
    {
        if (m_announceCallback != null)
            m_announceCallback();
    }
}
