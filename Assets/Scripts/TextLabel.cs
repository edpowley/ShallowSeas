using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextLabel : MonoBehaviour
{
    public RectTransform GuiObject;
    public bool m_stayOnScreen = false;

    public CanvasGroup m_canvasGroup;
    public Text m_text;

    private float m_duration = float.PositiveInfinity;
    private float m_lifetime = 0;

    void Start()
    {
        GuiObject.SetParent(GameManager.Instance.m_hudCanvas.transform, worldPositionStays: false);
    }

    internal void ShowMessage(string msg, float fadeTime)
    {
        m_text.text = msg;
        m_duration = fadeTime;
        m_lifetime = 0;
    }

    void Update()
    {
        if (!float.IsPositiveInfinity(m_duration))
        {
            if (m_lifetime < m_duration)
            {
                m_canvasGroup.alpha = 1.0f - m_lifetime / m_duration;
                m_lifetime += Time.deltaTime;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }

    void LateUpdate()
    {
        Vector3 vpPoint = GameManager.Instance.m_mainCamera.WorldToViewportPoint(transform.position);

		var hudCanvasRect = GameManager.Instance.m_hudCanvas.GetComponent<RectTransform>();
        Vector2 canvasSize = hudCanvasRect.sizeDelta;
        Vector2 canvasPoint = new Vector2((vpPoint.x - 0.5f) * canvasSize.x, (vpPoint.y - 0.5f) * canvasSize.y);

        GuiObject.anchoredPosition = canvasPoint;

        if (m_stayOnScreen)
        {
            Rect canvasRect = hudCanvasRect.GetComponent<RectTransform>().rect;
            float offX = 0, offY = 0;

            if (GuiObject.offsetMin.x < canvasRect.xMin)
                offX = canvasRect.xMin - GuiObject.offsetMin.x;
            else if (GuiObject.offsetMax.x > canvasRect.xMax)
                offX = canvasRect.xMax - GuiObject.offsetMax.x;

            if (GuiObject.offsetMin.y < canvasRect.yMin)
                offY = canvasRect.yMin - GuiObject.offsetMin.y;
            else if (GuiObject.offsetMax.y > canvasRect.yMax)
                offY = canvasRect.yMax - GuiObject.offsetMax.y;

            GuiObject.anchoredPosition += new Vector2(offX, offY);
        }
    }

    void OnDestroy()
    {
        Destroy(GuiObject.gameObject);
    }
}
