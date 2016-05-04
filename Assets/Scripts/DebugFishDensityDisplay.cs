using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ShallowNet;

public class DebugFishDensityDisplay : MonoBehaviour
{
    private Renderer m_renderer;
    public Renderer m_waterRenderer;
    private bool m_isShown;

    private Texture2D m_texture = null;

    void Start()
    {
        m_renderer = GetComponent<Renderer>();
        m_isShown = false;
    }

	private void showOrHide(bool show)
	{
		m_isShown = show;
		m_renderer.enabled = m_isShown;
		//m_waterRenderer.enabled = !m_isShown;
		transform.position = new Vector3(GameManager.Instance.MapWidth * 0.5f, transform.position.y, GameManager.Instance.MapHeight * 0.5f);
		transform.localScale = 0.1f * new Vector3(GameManager.Instance.MapWidth, GameManager.Instance.MapWidth, GameManager.Instance.MapHeight);

		if (m_isShown && m_texture == null)
		{
			m_texture = new Texture2D(GameManager.Instance.MapWidth, GameManager.Instance.MapHeight, TextureFormat.ARGB32, /*mipmap*/ false);
			m_texture.filterMode = FilterMode.Point;

			m_renderer.material.mainTexture = m_texture;
			m_renderer.material.renderQueue = 3100;
		}

		if (m_isShown)
		{
			RequestFishDensity msg = new RequestFishDensity() { X = 0, Y = 0, Width = GameManager.Instance.MapWidth, Height = GameManager.Instance.MapHeight };
			MyNetworkManager.Instance.m_client.sendMessage(msg);
		}
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            showOrHide(!m_isShown);
        }

        if (m_isShown)
            updateTexture();
    }

    private void updateTexture()
    {
        Color32[] colours = new Color32[GameManager.Instance.MapWidth * GameManager.Instance.MapHeight];
        Color32 transparent = new Color32(0, 0, 0, 0);

        for (int x = 0; x < GameManager.Instance.MapWidth; x++)
        {
            for (int y = 0; y < GameManager.Instance.MapHeight; y++)
            {
                List<float> density = GameManager.Instance.getFishDensity(x, y);
                Color32 colour;
                if (density != null)
                {
                    colour.r = (byte)(Mathf.Clamp01(density[0]) * 255);
                    colour.g = (byte)(Mathf.Clamp01(density[1]) * 255);
                    colour.b = (byte)(Mathf.Clamp01(density[2]) * 255);
                    colour.a = 255;
                }
                else
                {
                    colour = transparent;
                }

                colours[x + y * GameManager.Instance.MapWidth] = colour;
            }
        }

        m_texture.SetPixels32(colours);
        m_texture.Apply();
    }
}
