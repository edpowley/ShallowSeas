using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class FogCircle : MonoBehaviour
{
    private Mesh m_mesh;
    public Boat Boat;
    public int Radius = 10;
    public bool CheckLineOfSight = false;

    private IntVector2 m_centre = new IntVector2(-1, -1);

    void Start()
    {
        m_mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = m_mesh;
    }

    void Update()
    {
        IntVector2 boatCell = Boat.CurrentCell;
        if (boatCell != m_centre)
        {
            m_centre = boatCell;
            updateMesh();
            transform.position = new Vector3(m_centre.X, transform.position.y, m_centre.Y);
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
                if (sx*sx + sy*sy <= maxSqrRadius && GameManager.Instance.isWater(m_centre.X + sx, m_centre.Y + sy))
                {
                    squares[sx+Radius, sy+Radius] = true;
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
                    if (squares[sx+Radius, sy+Radius])
                    {
                        foreach (IntVector2 p in Util.SupercoverLine(0.5f, 0.5f, sx+0.5f, sy+0.5f))
                        {
                            if (!squares[p.X+Radius, p.Y+Radius])
                            {
                                squares[sx+Radius, sy+Radius] = false;
                                numSquares--;
                                break;
                            }
                        }
                    }
                }
            }
        }

        Debug.LogFormat("{0} squares = {1} vertices, {2} triangles", numSquares, numSquares * 4, numSquares * 2);

        Vector3[] vertices = new Vector3[numSquares * 4];
        Vector2[] uvs = new Vector2[numSquares * 4];
        int[] triangles = new int[numSquares * 6];

        int squareIndex = 0;
        for (int sx = -Radius; sx <= Radius; sx++)
        {
            for (int sy = -Radius; sy <= Radius; sy++)
            {
                if (squares[sx+Radius, sy+Radius])
                {
                    vertices[squareIndex*4+0] = new Vector3(sx+0, 0, sy+0);
                    vertices[squareIndex*4+1] = new Vector3(sx+0, 0, sy+1);
                    vertices[squareIndex*4+2] = new Vector3(sx+1, 0, sy+1);
                    vertices[squareIndex*4+3] = new Vector3(sx+1, 0, sy+0);

                    uvs[squareIndex*4+0] = new Vector2(sx+0, sy+0);
                    uvs[squareIndex*4+1] = new Vector2(sx+0, sy+1);
                    uvs[squareIndex*4+2] = new Vector2(sx+1, sy+1);
                    uvs[squareIndex*4+3] = new Vector2(sx+1, sy+0);

                    triangles[squareIndex*6+0] = squareIndex*4+0;
                    triangles[squareIndex*6+1] = squareIndex*4+1;
                    triangles[squareIndex*6+2] = squareIndex*4+2;
                    triangles[squareIndex*6+3] = squareIndex*4+0;
                    triangles[squareIndex*6+4] = squareIndex*4+2;
                    triangles[squareIndex*6+5] = squareIndex*4+3;

                    squareIndex++;
                }
            }
        }

        m_mesh.Clear();
        m_mesh.vertices = vertices;
        m_mesh.uv = uvs;
        m_mesh.triangles = triangles;
        m_mesh.RecalculateNormals();
    }
}
