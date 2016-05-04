using UnityEngine;
using System.Collections;

/// <summary>
/// Camera control script for tracking and zooming on an object.
/// </summary>
public class CameraZoom : MonoBehaviour
{
    private Camera m_camera;
    private bool m_isZoomInProgress;
    private Vector3 m_lastDragPos;

    /// <summary>
    /// Zoom level: 0 = max zoom in, 1 = max zoom out
    /// </summary>
    [Range(0, 2)]
    public float Zoom = 0.0f;

    public void setZoom(float zoom)
    {
        Zoom = zoom;
    }

    /// <summary>
    /// Mouse zoom sensitivity, in zoom units per pixel moved
    /// </summary>
    public float ZoomDragScale = 0.001f;

    public float ZoomWheelScale = 0.001f;

    /// <summary>
    /// Offset vector of the camera (at zoom 0) from the target
    /// </summary>
    public Vector3 Offset = new Vector3(0, 2, -4);

    void Start()
    {
        m_camera = GetComponent<Camera>();
        m_isZoomInProgress = false;

    }

    void LateUpdate()
    {
        if (m_isZoomInProgress)
        {
            if (Input.GetMouseButtonUp(1)) // 1 = right mouse button
            {
                // Mouse button released -- stop zoom
                m_isZoomInProgress = false;
            }
            else
            {
                // Mouse button still pressed update zoom
                Vector3 delta = m_lastDragPos - Input.mousePosition;
                Zoom += delta.y * ZoomDragScale;
                m_lastDragPos = Input.mousePosition;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                // Mouse button pressed -- start zoom
                m_isZoomInProgress = true;
                m_lastDragPos = Input.mousePosition;
            }
        }

        Zoom += Input.mouseScrollDelta.y * ZoomWheelScale;
        Zoom = Mathf.Clamp(Zoom, 0.0f, 2.0f);

        // Get target position
        Vector3 targetPos;
        if (GameManager.Instance != null && GameManager.Instance.LocalPlayerBoat != null)
            targetPos = GameManager.Instance.LocalPlayerBoat.transform.position;
        else
            targetPos = new Vector3(GameManager.Instance.MapWidth / 2, 0, GameManager.Instance.MapHeight / 2);

		float maxZoomY = 0.5f * GameManager.Instance.MapHeight / Mathf.Tan(0.5f * m_camera.fieldOfView * Mathf.Deg2Rad);
		float midZoomY = 0.5f * maxZoomY;

		// Update camera position and rotation
		Vector3 lowZoomPos = new Vector3(targetPos.x, Mathf.Clamp01(Zoom) * midZoomY, targetPos.z) + Offset;

        if (Zoom <= 1.0f)
        {
            transform.position = lowZoomPos;
            transform.LookAt(targetPos, Vector3.up);
        }
        else
        {
            Vector3 maxZoomPos = new Vector3(GameManager.Instance.MapWidth * 0.5f, maxZoomY, GameManager.Instance.MapHeight * 0.5f);

            transform.position = Vector3.Lerp(lowZoomPos, maxZoomPos, Zoom - 1);

            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }
}
