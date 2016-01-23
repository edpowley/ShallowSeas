using UnityEngine;
using System.Collections;

public class FpsCounter : MonoBehaviour
{
	private float m_elapsedTime;
	private int m_elapsedFrames;
	private GUIText m_oldText; // pre Unity 4.6
	private UnityEngine.UI.Text m_newText; // post Unity 4.6

	void Start()
	{
		m_oldText = GetComponent<GUIText>();
		m_newText = GetComponent<UnityEngine.UI.Text>();
	}

	// Update is called once per frame
	void Update()
	{
		m_elapsedFrames++;
		m_elapsedTime += Time.unscaledDeltaTime;

		if (m_elapsedTime >= 1.0f)
		{
			float fps = m_elapsedFrames / m_elapsedTime;
			m_elapsedFrames = 0;
			m_elapsedTime = 0;

			string text = string.Format("{0:0.00} FPS", fps);
			if (m_oldText != null) m_oldText.text = text;
			if (m_newText != null) m_newText.text = text;
		}
	}
}
