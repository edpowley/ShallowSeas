﻿using UnityEngine;
using System.Collections;

public class GridOverlay : MonoBehaviour
{
    private Texture2D m_maskTexture;

    // Use this for initialization
    void Start()
    {
        m_maskTexture = new Texture2D(GameManager.Instance.MapWidth, GameManager.Instance.MapHeight, TextureFormat.Alpha8, false);
        m_maskTexture.filterMode = FilterMode.Point;
        m_maskTexture.wrapMode = TextureWrapMode.Clamp;

        GameManager gm = GameManager.Instance;

        for (int x = 0; x < GameManager.Instance.MapWidth; x++)
        {
            for (int y = 0; y < GameManager.Instance.MapHeight; y++)
            {
                m_maskTexture.SetPixel(x, y, gm.isWater(x, y) ? Color.white : Color.clear);
            }
        }

        m_maskTexture.Apply();

        GetComponent<MeshRenderer>().material.SetTexture("_MaskTex", m_maskTexture);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
