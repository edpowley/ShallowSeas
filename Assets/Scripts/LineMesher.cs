//#define DEBUG_Z_OFFSET
#define ADD_OFFSETS_TO_VERTICES

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class LineMesher
{
	public float c_radius = 0.25f;
	private const bool c_addOffsetsToVertices = false;

	private List<Vector3> m_vertices = new List<Vector3>();
	private List<Vector2> m_uvs = new List<Vector2>();
	private List<Vector2> m_offs = new List<Vector2>();
	private List<int> m_triangles = new List<int>();

	public LineMesher()
	{
	}

	public void Clear()
	{
		m_vertices.Clear();
		m_uvs.Clear();
		m_offs.Clear();
		m_triangles.Clear();
	}

	public void PopulateMesh(Mesh mesh)
	{
		mesh.Clear();

		Vector3[] vertices = new Vector3[m_vertices.Count];
		for (int i = 0; i < m_vertices.Count; i++)
		{
			vertices[i] = m_vertices[i];
#if ADD_OFFSETS_TO_VERTICES
			vertices[i] += (Vector3)m_offs[i];
#endif
		}

		mesh.vertices = vertices;
		mesh.uv = m_uvs.ToArray();
		mesh.uv2 = m_offs.ToArray();
		//mesh.uv1 = Enumerable.Repeat(Vector2.up, m_vertices.Count).ToArray();
		mesh.normals = Enumerable.Repeat(Vector3.back, m_vertices.Count).ToArray();
		mesh.triangles = m_triangles.ToArray();
	}

	public Mesh CreateMesh()
	{
		Mesh mesh = new Mesh();
		PopulateMesh(mesh);
		return mesh;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="buffer"></param>
	/// <returns>If buffer is non-null and sufficiently large, then the number of vertices. If not, then the required buffer size expressed as a negative number.</returns>
	public int GetUIVertices(UIVertex[] buffer)
	{
		int numVertices = m_triangles.Count / 6 * 4;
		if (buffer == null || buffer.Length < numVertices)
			return -numVertices;

		for (int i = 0; i < m_triangles.Count / 6; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				int v = m_triangles[i * 6 + j];
				UIVertex vertex = new UIVertex();

				vertex.position = m_vertices[v];
#if ADD_OFFSETS_TO_VERTICES
				vertex.position += (Vector3)m_offs[v];
#endif
				vertex.normal = Vector3.back;
				vertex.uv0 = m_uvs[v];
				vertex.uv1 = m_offs[v];
				vertex.color = new Color32(255, 255, 255, 255);

				buffer[i * 4 + j] = vertex;
			}
		}

		return numVertices;
	}

	public void AddLineStripAsIndividualLines(List<Vector2> points)
	{
		for (int i = 0; i < points.Count - 1; i++)
			AddLineStrip(new List<Vector2> { points[i], points[i + 1] });
	}

    static private float signedAngle(Vector2 a, Vector2 b)
    {
        if (a.x == 0 && a.y == 0) return 0;
        if (b.x == 0 && b.y == 0) return 0;
        
        float angle = Vector2.Angle(a, b);
        if (a.x * b.y - a.y * b.x < 0)
            angle = -angle;
        
        while (angle > 180)
            angle -= 360;
        
        while (angle < -180)
            angle += 360;
        
        return angle;
    }

    public void AddLineStripAsSegmentsByAngle(List<Vector2> points, float angleThresh = 30)
	{
		if (points.Count < 2)
		{
			AddLineStrip(points);
		}
		else
		{
			List<Vector2> segment = new List<Vector2> { points[0], points[1] };

			for (int i = 2; i < points.Count; i++)
			{
				Vector2 a = segment[segment.Count - 1] - segment[segment.Count - 2];
				Vector2 b = points[i] - segment[segment.Count - 1];
				if (b != Vector2.zero)
				{
					float angle = signedAngle(a, b);
					if (Mathf.Abs(angle) >= angleThresh)
					{
						AddLineStrip(segment);
						segment.Clear();
						segment.Add(points[i - 1]);
						segment.Add(points[i]);
					}
					else
					{
						segment.Add(points[i]);
					}
				}
			}

			AddLineStrip(segment);
		}
	}

	public void AddLineLoop(List<Vector2> points)
	{
		points = new List<Vector2>(points);
		points.Add(points[0]);
		AddLineStrip(points);
	}

	private int addVertex(Vector2 xy, Vector2 off, Vector2 uv)
	{
		if (off.sqrMagnitude > 1000000.0f)
			Debug.LogWarning("Vertex is a long way from origin");

#if DEBUG_Z_OFFSET
		m_vertices.Add(new Vector3(xy.x+off.x, xy.y+off.y, -m_vertices.Count * 0.01f));
#else
		m_vertices.Add(xy);
#endif
		m_uvs.Add(uv);
		m_offs.Add(off);
		return m_vertices.Count - 1;
	}

	private void addQuad(int v0, int v1, int v2, int v3)
	{
		m_triangles.Add(v0);
		m_triangles.Add(v1);
		m_triangles.Add(v2);
		m_triangles.Add(v3);
		m_triangles.Add(v0);
		m_triangles.Add(v2);
	}

	public void AddLineStrip(List<Vector2> points)
	{
		if (points.Count == 0)
		{
			// Do nothing
		}
		else if (points.Count == 1)
		{
			Vector2 point = points.Single();
			int cap1 = addVertex(point, new Vector2(-c_radius, -c_radius), new Vector2(0, 0));
			int cap2 = addVertex(point, new Vector2(-c_radius, +c_radius), new Vector2(0, 1));
			int cap3 = addVertex(point, new Vector2(+c_radius, -c_radius), new Vector2(1, 0));
			int cap4 = addVertex(point, new Vector2(+c_radius, +c_radius), new Vector2(1, 1));

			addQuad(cap1, cap3, cap2, cap4);
		}
		else
		{
			Vector2 pointA, pointB, pointC, tangent, normal;
			int cap1, cap2, u1, u2, v1, v2;

			// Start cap
			pointA = points[0];
			pointB = points[1];

			tangent = (pointB - pointA).normalized * c_radius;
			normal = new Vector2(-tangent.y, tangent.x);

			Vector2 tex1 = new Vector2(0.5f, 0.0f);
			Vector2 texMid = new Vector2(0.5f, 0.5f);
			Vector2 tex2 = new Vector2(0.5f, 1.0f);
			Vector2 tex3 = new Vector2(0.0f, 0.0f);
			Vector2 tex4 = new Vector2(0.0f, 1.0f);
			Vector2 tex5 = new Vector2(1.0f, 0.0f);
			Vector2 tex6 = new Vector2(1.0f, 1.0f);

			cap1 = addVertex(pointA, normal - tangent, tex3);
			cap2 = addVertex(pointA, -normal - tangent, tex4);
			u1 = addVertex(pointA, normal, tex1);
			u2 = addVertex(pointA, -normal, tex2);

			addQuad(cap1, u1, u2, cap2);

			// Segments
			for (int i = 1; i < points.Count - 1; i++)
			{
				pointA = points[i - 1];
				pointB = points[i];
				pointC = points[i + 1];

				Vector2 tangentAB = (pointB - pointA).normalized * c_radius;
				Vector2 normalAB = new Vector2(-tangentAB.y, tangentAB.x);
				Vector2 tangentBC = (pointC - pointB).normalized * c_radius;
				Vector2 normalBC = new Vector2(-tangentBC.y, tangentBC.x);

				LineIntersection intersect1 = new LineIntersection(pointA + normalAB, tangentAB, pointB + normalBC, tangentBC);
				LineIntersection intersect2 = new LineIntersection(pointA - normalAB, tangentAB, pointB - normalBC, tangentBC);
				//if (intersect1.InfiniteLinesIntersect && intersect2.InfiniteLinesIntersect)
				{
					float c_maxBevelDistance = c_radius * 6;

					Vector2 ip1 = intersect1.point;
					Vector2 ipTex1 = tex1;
					if ((ip1 - pointB).sqrMagnitude > c_maxBevelDistance * c_maxBevelDistance)
					{
						float scale = c_maxBevelDistance / (ip1 - pointB).magnitude;
						ip1 = pointB + (ip1 - pointB) * scale;
						ipTex1 = texMid + (ipTex1 - texMid) * scale;
					}

					Vector2 ip2 = intersect2.point;
					Vector2 ipTex2 = tex2;
					if ((ip2 - pointB).sqrMagnitude > c_maxBevelDistance * c_maxBevelDistance)
					{
						float scale = c_maxBevelDistance / (ip2 - pointB).magnitude;
						ip2 = pointB + (ip2 - pointB) * scale;
						ipTex2 = texMid + (ipTex2 - texMid) * scale;
					}

					v1 = addVertex(pointB, ip1 - pointB, ipTex1);
					v2 = addVertex(pointB, ip2 - pointB, ipTex2);

					addQuad(u1, v1, v2, u2);

					u1 = v1; u2 = v2;
				}
			}

			// Last segment
			pointA = points[points.Count - 2];
			pointB = points[points.Count - 1];
			tangent = (pointB - pointA).normalized * c_radius;
			normal = new Vector2(-tangent.y, tangent.x);

			v1 = addVertex(pointB, normal, tex1);
			v2 = addVertex(pointB, -normal, tex2);
			addQuad(u1, v1, v2, u2);

			// End cap
			cap1 = addVertex(pointB, normal + tangent, tex5);
			cap2 = addVertex(pointB, -normal + tangent, tex6);
			addQuad(v1, cap1, cap2, v2);
		}
	}
}
