using UnityEngine;
using System.Collections;

public class TextLabel : MonoBehaviour
{
    public string CanvasName = "Canvas";
    public string SceneCameraName = "Main Camera";
    public RectTransform GuiObject;

    private Camera m_sceneCamera;
    private Canvas m_canvas;

	// Use this for initialization
	void Start ()
    {
        m_sceneCamera = GameObject.Find(SceneCameraName).GetComponent<Camera>();
        m_canvas = GameObject.Find(CanvasName).GetComponent<Canvas>();

        GuiObject.SetParent(m_canvas.transform, worldPositionStays: false);
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 vpPoint = m_sceneCamera.WorldToViewportPoint(transform.position);

        Vector2 canvasSize = m_canvas.GetComponent<RectTransform>().sizeDelta;
        Vector2 canvasPoint = new Vector2((vpPoint.x - 0.5f) * canvasSize.x, (vpPoint.y - 0.5f) * canvasSize.y);

        GuiObject.anchoredPosition = canvasPoint;
	}
}
