using UnityEngine;
using System.Collections;

public class GridOverlay : MonoBehaviour {

    private Texture2D m_maskTexture;

	// Use this for initialization
	void Start ()
    {
        m_maskTexture = new Texture2D(1024, 1024);
        m_maskTexture.filterMode = FilterMode.Point;
        m_maskTexture.wrapMode = TextureWrapMode.Clamp;

        Terrain terrain = Terrain.activeTerrain;

        for (int x=0; x<1024; x++)
        {
            for (int y=0; y<1024; y++)
            {
                float height = terrain.SampleHeight(new Vector3(x-512, 0, y-512));
                bool isWater = (height < 63.5f);
                m_maskTexture.SetPixel(x, y, isWater ? Color.white : Color.clear);
                //m_maskTexture.SetPixel(x, y, new Color(x/1024.0f, y/1024.0f, Random.Range(0.0f, 1.0f)));
            }
        }

        m_maskTexture.Apply();

        GetComponent<MeshRenderer>().material.SetTexture("_MaskTex", m_maskTexture);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
