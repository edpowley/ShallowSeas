using UnityEngine;
using System.Collections;

public class TextLabel : MonoBehaviour
{
    public string CanvasName = "Canvas";
    public string SceneCameraName = "Main Camera";
    public RectTransform GuiObject;
    public bool m_stayOnScreen = false;

    private Camera m_sceneCamera;
    private Canvas m_canvas;

	void Start()
    {
        m_sceneCamera = GameObject.Find(SceneCameraName).GetComponent<Camera>();
        m_canvas = GameObject.Find(CanvasName).GetComponent<Canvas>();

        GuiObject.SetParent(m_canvas.transform, worldPositionStays: false);
	}
	
	void LateUpdate()
    {
        Vector3 vpPoint = m_sceneCamera.WorldToViewportPoint(transform.position);

        Vector2 canvasSize = m_canvas.GetComponent<RectTransform>().sizeDelta;
        Vector2 canvasPoint = new Vector2((vpPoint.x - 0.5f) * canvasSize.x, (vpPoint.y - 0.5f) * canvasSize.y);

        GuiObject.anchoredPosition = canvasPoint;

        if (m_stayOnScreen)
        {
            Rect canvasRect = m_canvas.GetComponent<RectTransform>().rect;
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
}
