using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Intersection between two lines
/// </summary>
/// <remarks>
/// See Gareth Rees' answer on http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
/// </remarks>
public struct LineIntersection
{
    static private float cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    public LineIntersection(Vector2 pointA, Vector2 dirA, Vector2 pointB, Vector2 dirB)
	{
		float dAxdB = cross(dirA, dirB);
		if (Mathf.Abs(dAxdB) < Vector2.kEpsilon)
		{
			if (Mathf.Abs(cross(pointB - pointA, dirA)) < Vector2.kEpsilon)
			{
				tA = Vector2.Dot(pointB - pointA, dirA) / dirA.sqrMagnitude;
				tB = Vector2.Dot(pointA - pointB, dirB) / dirB.sqrMagnitude;

				if (tA >= 0 && tA <= 1 && tB >= 0 && tB <= 1)
				{
					this.point = pointA + tA * dirA;
					intersectionType = IntersectionType.Overlapping;
				}
				else
				{
					this.point = pointA + tA * dirA;
					intersectionType = IntersectionType.Collinear;
				}
			}
			else
			{
				intersectionType = IntersectionType.Parallel;
				tA = tB = float.NaN;
				this.point = new Vector2(float.NaN, float.NaN);
			}
		}
		else
		{
			tA = cross(pointB - pointA, dirB) / dAxdB;
			tB = cross(pointA - pointB, dirA) / -dAxdB;
			point = pointA + tA * dirA;

			if (tA >= 0 && tA <= 1 && tB >= 0 && tB <= 1)
				intersectionType = IntersectionType.Intersecting;
			else
				intersectionType = IntersectionType.NonIntersecting;
		}
	}

	public readonly float tA, tB;
	public readonly Vector2 point;

	public enum IntersectionType { Overlapping, Collinear, Parallel, Intersecting, NonIntersecting }
	public readonly IntersectionType intersectionType;

	public bool LineSegmentsIntersect
	{
		get { return intersectionType == IntersectionType.Overlapping || intersectionType == IntersectionType.Intersecting; }
	}

	public bool InfiniteLinesIntersect
	{
		get { return intersectionType != IntersectionType.Parallel; }
	}
}
