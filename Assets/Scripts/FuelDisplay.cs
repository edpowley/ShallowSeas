using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FuelDisplay : MonoBehaviour {

	public LayoutElement m_mainBar, m_usingBar;
	public float m_width;

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		var boat = GameManager.Instance.LocalPlayerBoat;
		if (boat!=null)
		{
			float scale = m_width / (float)boat.m_initialFuel;
			m_mainBar.minWidth = boat.m_remainingFuel * scale;

			if (boat.m_course.Count > 0 && boat.m_courseEndTime > GameManager.Instance.CurrentTime)
			{
				float distanceLeft = (boat.m_courseEndTime - GameManager.Instance.CurrentTime) * boat.m_movementSpeed;
				Debug.LogFormat("distanceLeft = {0}", distanceLeft);
				m_usingBar.minWidth = distanceLeft * scale;
				m_mainBar.minWidth -= m_usingBar.minWidth;
			}
			else
			{
				Debug.LogFormat("no path");
				m_usingBar.minWidth = 0;
			}
		}
	}
}
