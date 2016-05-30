using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class FuelDisplay : MonoBehaviour
{

	public LayoutElement m_mainBar, m_usingBar;
	public float m_width;

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
			float distanceLeft;

			var drawingCourse = GameManager.Instance.DrawingLine.Course;
			if (drawingCourse.Any())
			{
				distanceLeft = 0;
				var lastPoint = drawingCourse.First();
				foreach (var point in drawingCourse.Skip(1))
				{
					distanceLeft += (point - lastPoint).magnitude;
					lastPoint = point;
				}
			}
			else if (boat.m_course.Count > 0 && boat.m_courseEndTime > GameManager.Instance.CurrentTime)
			{
				distanceLeft = (boat.m_courseEndTime - GameManager.Instance.CurrentTime) * boat.m_movementSpeed;
			}
			else
			{
				distanceLeft = 0;
			}

			distanceLeft = Mathf.Min(distanceLeft, boat.m_remainingFuel);

			float scale = m_width / (float)boat.m_initialFuel;
			m_mainBar.minWidth = (boat.m_remainingFuel - distanceLeft) * scale;
			m_usingBar.minWidth = distanceLeft * scale;
		}
	}
}
