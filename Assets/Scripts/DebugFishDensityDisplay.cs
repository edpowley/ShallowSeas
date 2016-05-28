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

	private const int c_numColours = 64;
	private Color32[] m_fishMapColours;

	private void initFishMapColours()
	{
		m_fishMapColours = new Color32[c_numColours];
		for (int i = 0; i < c_numColours; i++)
		{
			double fraction = (double)i / (double)c_numColours;

			// Colour formula from cam.vogl.c function fraction2rgb
			double hue = 1.0 - fraction;
			if (hue < 0.0) hue = 0.0;
			if (hue > 1.0) hue = 1.0;
			int huesector = (int)System.Math.Floor(hue * 5.0);
			double huetune = hue * 5.0 - huesector;
			double mix_up = huetune;
			double mix_do = 1.0 - huetune;
			mix_up = System.Math.Pow(mix_up, 1.0 / 2.5);
			mix_do = System.Math.Pow(mix_do, 1.0 / 2.5);
			double r, g, b;
			switch (huesector)
			{
				case 0: r = 1.0; g = mix_up; b = 0.0; break; /* red    to yellow */
				case 1: r = mix_do; g = 1.0; b = 0.0; break; /* yellow to green  */
				case 2: r = 0.0; g = 1.0; b = mix_up; break; /* green  to cyan   */
				case 3: r = 0.0; g = mix_do; b = 1.0; break; /* cyan   to blue   */
				case 4: r = 0.0; g = 0.0; b = mix_do; break; /* blue   to black  */
				default: r = 0.0; g = 0.0; b = 0.0; break;
			}

			m_fishMapColours[i] = new Color32((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), 255);
		}
	}

	void Start()
    {
        m_renderer = GetComponent<Renderer>();
        m_isShown = false;

		initFishMapColours();

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
						int colourIndex = (int)density[ft];
						if (colourIndex < 0)
							colour = m_fishMapColours[0];
						else if (colourIndex >= c_numColours)
							colour = m_fishMapColours[c_numColours - 1];
						else
							colour = m_fishMapColours[colourIndex];
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
