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

		StartCoroutine(refresh());
    }

	private void showOrHide(bool show)
	{
		m_isShown = show;
		m_renderer.enabled = m_isShown;
		//m_waterRenderer.enabled = !m_isShown;
		transform.position = new Vector3(GameManager.Instance.MapWidth * 0.5f + 0.125f, transform.position.y, GameManager.Instance.MapHeight * 0.5f + 0.125f);
		transform.localScale = 0.1f * new Vector3(GameManager.Instance.MapWidth, GameManager.Instance.MapWidth, GameManager.Instance.MapHeight);

		if (m_isShown && m_texture == null)
		{
			m_texture = new Texture2D(GameManager.Instance.MapWidth * 4, GameManager.Instance.MapHeight * 4, TextureFormat.ARGB32, /*mipmap*/ false);
			m_texture.filterMode = FilterMode.Point;

			m_renderer.material.mainTexture = m_texture;
			m_renderer.material.renderQueue = 3100;
		}
	}

	private IEnumerator refresh()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.5f);

			if (m_isShown)
			{
				RequestFishDensity msg = new RequestFishDensity() { X = 0, Y = 0, Width = GameManager.Instance.MapWidth, Height = GameManager.Instance.MapHeight };
				MyNetworkManager.Instance.m_client.sendMessage(msg);
			}
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
        Color32[] colours = new Color32[GameManager.Instance.MapWidth * GameManager.Instance.MapHeight * 4 * 4];
        Color32 transparent = new Color32(0, 0, 0, 0);

        for (int x = 0; x < GameManager.Instance.MapWidth; x++)
        {
            for (int y = 0; y < GameManager.Instance.MapHeight; y++)
            {
                var density = GameManager.Instance.getFishDensity(x, y);
				if (density != null)
				{
					int i = 0;
					foreach(FishType ft in FishType.All)
					{
						int px = x * 4 + (i % 3);
						int py = y * 4 + (i / 3);
						Color32 colour;
						colour.r = (byte)(Mathf.Clamp01(density[ft]) * 1000);
						colour.g = colour.r;
						colour.b = colour.r;
						colour.a = 255;
						colours[py * GameManager.Instance.MapWidth * 4 + px] = colour;
						i++;
					}
				}
            }
        }

        m_texture.SetPixels32(colours);
        m_texture.Apply();
    }
}
