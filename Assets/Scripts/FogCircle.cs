using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class FogCircle : MonoBehaviour
{
    private Mesh m_mesh;
    public int Radius = 10;
    public bool CheckLineOfSight = false;

    private IntVector2 m_centre = new IntVector2(-1, -1);
    private HashSet<IntVector2> m_visibleCells = new HashSet<IntVector2>();

    private Renderer m_renderer;

    void Start()
    {
        m_renderer = GetComponent<Renderer>();
        m_renderer.enabled = false;

        m_mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = m_mesh;
    }

    void Update()
    {
        if (GameManager.Instance.LocalPlayerBoat != null)
        {
            m_renderer.enabled = true;

            IntVector2 boatCell = GameManager.Instance.LocalPlayerBoat.CurrentCell;
            if (boatCell != m_centre)
            {
                m_centre = boatCell;
                transform.position = new Vector3(m_centre.X, transform.position.y, m_centre.Y);
                updateMesh();
            }
        }
        else
        {
            m_renderer.enabled = false;
        }
    }

    private void updateMesh()
    {
        int diameter = Radius * 2 + 1;
        bool[,] squares = new bool[diameter, diameter];
        int numSquares = 0;
        float maxSqrRadius = (Radius + 0.5f) * (Radius + 0.5f);

        for (int sx = -Radius; sx <= Radius; sx++)
        {
            for (int sy = -Radius; sy <= Radius; sy++)
            {
                if (sx * sx + sy * sy <= maxSqrRadius && GameManager.Instance.isWater(m_centre.X + sx, m_centre.Y + sy))
                {
                    squares[sx + Radius, sy + Radius] = true;
                    numSquares++;
                }
            }
        }

        if (CheckLineOfSight)
        {
            for (int sx = -Radius; sx <= Radius; sx++)
            {
                for (int sy = -Radius; sy <= Radius; sy++)
                {
                    if (squares[sx + Radius, sy + Radius])
                    {
                        foreach (IntVector2 p in Util.SupercoverLine(0.5f, 0.5f, sx + 0.5f, sy + 0.5f))
                        {
                            if (!squares[p.X + Radius, p.Y + Radius])
                            {
                                squares[sx + Radius, sy + Radius] = false;
                                numSquares--;
                                break;
                            }
                        }
                    }
                }
            }
        }

        Vector3[] vertices = new Vector3[numSquares * 4];
        Vector2[] uvs = new Vector2[numSquares * 4];
        int[] triangles = new int[numSquares * 6];

        int squareIndex = 0;
        for (int sx = -Radius; sx <= Radius; sx++)
        {
            for (int sy = -Radius; sy <= Radius; sy++)
            {
                if (squares[sx + Radius, sy + Radius])
                {
                    vertices[squareIndex * 4 + 0] = new Vector3(sx + 0, 0, sy + 0);
                    vertices[squareIndex * 4 + 1] = new Vector3(sx + 0, 0, sy + 1);
                    vertices[squareIndex * 4 + 2] = new Vector3(sx + 1, 0, sy + 1);
                    vertices[squareIndex * 4 + 3] = new Vector3(sx + 1, 0, sy + 0);

                    uvs[squareIndex * 4 + 0] = new Vector2(sx + 0, sy + 0);
                    uvs[squareIndex * 4 + 1] = new Vector2(sx + 0, sy + 1);
                    uvs[squareIndex * 4 + 2] = new Vector2(sx + 1, sy + 1);
                    uvs[squareIndex * 4 + 3] = new Vector2(sx + 1, sy + 0);

                    triangles[squareIndex * 6 + 0] = squareIndex * 4 + 0;
                    triangles[squareIndex * 6 + 1] = squareIndex * 4 + 1;
                    triangles[squareIndex * 6 + 2] = squareIndex * 4 + 2;
                    triangles[squareIndex * 6 + 3] = squareIndex * 4 + 0;
                    triangles[squareIndex * 6 + 4] = squareIndex * 4 + 2;
                    triangles[squareIndex * 6 + 5] = squareIndex * 4 + 3;

                    squareIndex++;
                }
            }
        }

        m_mesh.Clear();
        m_mesh.vertices = vertices;
        m_mesh.uv = uvs;
        m_mesh.triangles = triangles;
        m_mesh.RecalculateNormals();

        m_visibleCells.Clear();

        for (int sx = -Radius; sx <= Radius; sx++)
        {
            for (int sy = -Radius; sy <= Radius; sy++)
            {
                if (squares[sx + Radius, sy + Radius])
                {
                    m_visibleCells.Add(m_centre + new IntVector2(sx, sy));
                }
            }
        }
    }

    internal bool cellIsVisible(IntVector2 cell)
    {
        return m_visibleCells.Contains(cell);
    }
}
