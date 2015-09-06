using System;

[Serializable]
public class GearInfo
{
    public string m_gearName;
    public float m_castDuration;
    public int m_maxCatch;
    public int[] m_catchMultiplier = new int[] { 1, 1, 1 };
}

