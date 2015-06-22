﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Camera control script for tracking and zooming on an object.
/// </summary>
[ExecuteInEditMode]
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

    /// <summary>
    /// Mouse zoom sensitivity, in zoom units per pixel moved
    /// </summary>
    public float ZoomDragScale = 0.001f;

    public float ZoomWheelScale = 0.001f;

    /// <summary>
    /// Offset vector of the camera (at zoom 0) from the target
    /// </summary>
    public Vector3 Offset = new Vector3(0, 2, -4);

    public float MidZoomY = 500;
    public Vector2 MaxZoomHalfArea = new Vector2(512, 512);

    /// <summary>
    /// The target
    /// </summary>
    public Transform Target;

    void Start()
    {
        m_camera = GetComponent<Camera>();
        m_isZoomInProgress = false;

    }
	
    void Update()
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
        if (Target != null)
            targetPos = Target.position;
        else 
            targetPos = Vector3.zero;

        // Update camera position and rotation
        Vector3 lowZoomPos = new Vector3(targetPos.x, Mathf.Clamp01(Zoom) * MidZoomY, targetPos.z) + Offset;

        if (Zoom <= 1.0f)
        {
            transform.position = lowZoomPos;
            transform.LookAt(targetPos, Vector3.up);
        }
        else
        {
            float maxZoomY = MaxZoomHalfArea.y / Mathf.Tan(0.5f * m_camera.fieldOfView * Mathf.Deg2Rad);
            Vector3 maxZoomPos = new Vector3(0, maxZoomY, 0);

            transform.position = Vector3.Lerp(lowZoomPos, maxZoomPos, Zoom - 1);

            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }
}