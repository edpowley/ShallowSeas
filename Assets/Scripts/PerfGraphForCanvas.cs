using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PerfGraphForCanvas : MonoBehaviour
{
    public float m_sampleTime = 5.0f;
    public float m_yUnit = 1.0f / 60.0f;

    private struct DataPoint { public float x, y; }
    private List<DataPoint> m_data = new List<DataPoint>();

    private RectTransform m_rectTransform;
    private CanvasRenderer m_renderer;
    public Material m_material;

    void OnEnable()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_renderer = GetComponent<CanvasRenderer>();
        m_renderer.SetMaterial(m_material, null);

        StartCoroutine(updateLoop());
    }

    // Update is called once per frame
    void Update()
    {
        m_data.Add(new DataPoint { x = Time.unscaledTime, y = Time.unscaledDeltaTime });
    }

    private IEnumerator updateLoop()
    {
        float lastUpdateTime = Time.unscaledTime;
        for (;;)
        {
            while (Time.unscaledTime < lastUpdateTime + 0.1f)
                yield return null;

            lastUpdateTime = Time.unscaledTime;

            removeOldPoints();
            updateMesh();
        }
    }
    
    private void removeOldPoints()
    {
        int numToRemove;
        float threshold = Time.unscaledTime - m_sampleTime;
        for (numToRemove = 0; numToRemove < m_data.Count; numToRemove++)
        {
            if (m_data[numToRemove].x >= threshold)
                break;
        }

        m_data.RemoveRange(0, numToRemove);
    }

    private void updateMesh()
    {
        Rect targetRect;
        if (m_rectTransform != null)
            targetRect = m_rectTransform.rect;
        else
            targetRect = new Rect(0, 0, 1, 1);

        float xScale = targetRect.width / m_sampleTime;
        float xOff = Time.unscaledTime - m_sampleTime;
        //float yScale = 1.0f / m_data.Max();
        float yScale = targetRect.height / m_yUnit * 0.25f;

        List<UIVertex> vertices = new List<UIVertex>();

        for (int i = 0; i < m_data.Count; i++)
        {
            DataPoint p = m_data[i];
            float x1 = (p.x - p.y - xOff) * xScale + targetRect.xMin;
            float x2 = (p.x - xOff) * xScale + targetRect.xMin;
            float y1 = targetRect.yMin;
            float y2 = p.y * yScale + targetRect.yMin;
            float v = p.y / m_yUnit * 0.5f;

            vertices.Add(new UIVertex { position = new Vector3(x1, y1, 0), uv0 = new Vector2(0, 0) });
            vertices.Add(new UIVertex { position = new Vector3(x2, y1, 0), uv0 = new Vector2(0, 0) });
            vertices.Add(new UIVertex { position = new Vector3(x2, y2, 0), uv0 = new Vector2(0, v) });
            vertices.Add(new UIVertex { position = new Vector3(x1, y2, 0), uv0 = new Vector2(0, v) });
        }

        m_renderer.SetVertices(vertices);
    }
}
