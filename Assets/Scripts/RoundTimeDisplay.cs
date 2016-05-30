using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RoundTimeDisplay : MonoBehaviour
{
	private Text m_text;

	// Use this for initialization
	void Start()
	{
		m_text = GetComponent<Text>();
	}

	// Update is called once per frame
	void Update()
	{
		float timeLeft = GameManager.Instance.m_roundEndTime - GameManager.Instance.CurrentTime;
		int secondsLeft = Mathf.CeilToInt(timeLeft);
		if (secondsLeft < 0)
			secondsLeft = 0;

		int minutesLeft = secondsLeft / 60;
		secondsLeft = secondsLeft % 60;

		m_text.text = string.Format("Time left today: {0}:{1:00}", minutesLeft, secondsLeft);
	}
}
