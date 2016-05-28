using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ShallowNet;
using UnityEngine.UI;

public class CatchDisplay : MonoBehaviour
{
	public List<LayoutElement> m_bars = new List<LayoutElement>();

	// Use this for initialization
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		var boat = GameManager.Instance.LocalPlayerBoat;
		if (boat != null)
		{
			float scale = this.GetComponent<RectTransform>().rect.width / (float)boat.m_maxCatch;
			for (int i = 0; i < FishType.All.Count; i++)
			{
				FishType ft = FishType.All[i];
				var bar = m_bars[i];
				int num = boat.m_catch[ft];
				bar.minWidth = num * scale;
			}
		}
	}
}
