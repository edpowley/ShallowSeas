using UnityEngine;
using System.Collections;

public class DebugFishDensityDisplay : MonoBehaviour
{
    private Renderer m_renderer;
    public Renderer m_waterRenderer;

    void Start()
    {
        m_renderer = GetComponent<Renderer>();
    }

	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            m_renderer.enabled = !m_renderer.enabled;
            m_waterRenderer.enabled = !m_renderer.enabled;
        }
	}
}
