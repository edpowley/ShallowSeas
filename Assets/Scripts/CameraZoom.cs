using UnityEngine;
using System.Collections;

public class CameraZoom : MonoBehaviour
{
    private Camera m_camera;
    private bool m_isZoomInProgress;
    private Vector3 m_lastDragPos;
    public float m_zoom = 0.0f;
    public float m_zoomDragScale = 0.001f;
    public Vector3 m_offset = new Vector3(0, 2, -4);

    public Transform m_target;

    void Start()
    {
        m_camera = GetComponent<Camera>();
        m_isZoomInProgress = false;
    }
	
    void Update()
    {
        if (m_isZoomInProgress)
        {
            if (Input.GetMouseButtonUp(1))
            {
                m_isZoomInProgress = false;
            }
            else
            {
                Vector3 delta = m_lastDragPos - Input.mousePosition;
                m_zoom += delta.y * m_zoomDragScale;
                m_zoom = Mathf.Clamp01(m_zoom);
                m_lastDragPos = Input.mousePosition;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                m_isZoomInProgress = true;
                m_lastDragPos = Input.mousePosition;
            }
        }

        Vector3 targetPos = m_target.position;
        transform.position = new Vector3(targetPos.x, m_zoom * 500, targetPos.z) + m_offset;
        transform.LookAt(targetPos, Vector3.up);
    }
}
