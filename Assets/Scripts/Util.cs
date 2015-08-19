using System;
using UnityEngine;
using System.Collections.Generic;

public static class Util
{
    static public T InstantiatePrefab<T>(T prefab) where T : Component
    {
        return InstantiatePrefab<T>(prefab, prefab.transform.position, prefab.transform.rotation);
    }
    
    static public GameObject InstantiatePrefab(GameObject prefab)
    {
        return InstantiatePrefab(prefab, prefab.transform.position, prefab.transform.rotation);
    }
    
    static public T InstantiatePrefab<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        GameObject gob = InstantiatePrefab(prefab.gameObject, position, rotation);
        return gob.GetComponent<T>();
    }
    
    static public GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return (GameObject)UnityEngine.Object.Instantiate(prefab, position, rotation);
    }

    /// <summary>
    /// Iterate all grid squares covered by the line from p1 to p2
    /// </summary>
    static public IEnumerable<IntVector2> SupercoverLine(Vector2 p1, Vector2 p2)
    {
        return SupercoverLine(p1.x, p1.y, p2.x, p2.y);
    }

    /// <summary>
    /// Iterate all grid squares covered by the line from (x1,y1) to (x2,y2)
    /// </summary>
    static public IEnumerable<IntVector2> SupercoverLine(float x1, float y1, float x2, float y2)
    {
        if (Mathf.FloorToInt(x1) == Mathf.FloorToInt(x2))
        {
            // Line is vertical
            int x = Mathf.FloorToInt(x1);
            int y = Mathf.FloorToInt(y1);
            int targetY = Mathf.FloorToInt(y2);
            int stepY = (targetY > y) ? +1 : -1;

            yield return new IntVector2(x,y);

            while (y != targetY)
            {
                y += stepY;
                yield return new IntVector2(x,y);
            }
        }
        else if (Mathf.FloorToInt(y1) == Mathf.FloorToInt(y2))
        {
            // Line is horizontal
            int x = Mathf.FloorToInt(x1);
            int y = Mathf.FloorToInt(y1);
            int targetX = Mathf.FloorToInt(x2);
            int stepX = (targetX > x) ? +1 : -1;
            
            yield return new IntVector2(x,y);
            
            while (x != targetX)
            {
                x += stepX;
                yield return new IntVector2(x,y);
            }
        }
        else
        {
            // General case algorithm is from http://www.cse.yorku.ca/~amana/research/grid.pdf
            float dx = x2 - x1;
            int stepX = (dx > 0) ? +1 : -1;
            float tDeltaX = 1.0f / Mathf.Abs(dx);
            float tMaxX;
            if (dx > 0)
                tMaxX = tDeltaX * (1.0f - x1 + Mathf.Floor(x1));
            else
                tMaxX = tDeltaX * (x1 - Mathf.Floor(x1));

            float dy = y2 - y1;
            int stepY = (dy > 0) ? +1 : -1;
            float tDeltaY = 1.0f / Mathf.Abs(dy);
            float tMaxY;
            if (dy > 0)
                tMaxY = tDeltaY * (1.0f - y1 + Mathf.Floor(y1));
            else
                tMaxY = tDeltaY * (y1 - Mathf.Floor(y1));

            int x = Mathf.FloorToInt(x1);
            int y = Mathf.FloorToInt(y1);
            int targetX = Mathf.FloorToInt(x2);
            int targetY = Mathf.FloorToInt(y2);

            yield return new IntVector2(x,y);

            while (x != targetX || y != targetY)
            {
                if (tMaxX < tMaxY)
                {
                    tMaxX += tDeltaX;
                    x += stepX;
                }
                else
                {
                    tMaxY += tDeltaY;
                    y += stepY;
                }

                yield return new IntVector2(x,y);

                if ((targetX - x) * stepX < 0 || (targetY - y) * stepY < 0)
                {
                    throw new InvalidOperationException("Overshoot");
                }
            }
        }
    }

    public static Color HSVToRGB(float H, float S, float V)
    {
        // http://answers.unity3d.com/questions/701956/hsv-to-rgb-without-editorguiutilityhsvtorgb.html

        if (S == 0f)
            return new Color(V,V,V);
        else if (V == 0f)
            return Color.black;
        else
        {
            Color col = Color.black;
            float Hval = H * 6f;
            int sel = Mathf.FloorToInt(Hval);
            float mod = Hval - sel;
            float v1 = V * (1f - S);
            float v2 = V * (1f - S * mod);
            float v3 = V * (1f - S * (1f - mod));
            switch (sel + 1)
            {
                case 0:
                    col.r = V;
                    col.g = v1;
                    col.b = v2;
                    break;
                case 1:
                    col.r = V;
                    col.g = v3;
                    col.b = v1;
                    break;
                case 2:
                    col.r = v2;
                    col.g = V;
                    col.b = v1;
                    break;
                case 3:
                    col.r = v1;
                    col.g = V;
                    col.b = v3;
                    break;
                case 4:
                    col.r = v1;
                    col.g = v2;
                    col.b = V;
                    break;
                case 5:
                    col.r = v3;
                    col.g = v1;
                    col.b = V;
                    break;
                case 6:
                    col.r = V;
                    col.g = v1;
                    col.b = v2;
                    break;
                case 7:
                    col.r = V;
                    col.g = v3;
                    col.b = v1;
                    break;
            }
            col.r = Mathf.Clamp(col.r, 0f, 1f);
            col.g = Mathf.Clamp(col.g, 0f, 1f);
            col.b = Mathf.Clamp(col.b, 0f, 1f);
            return col;
        }
    }
}

