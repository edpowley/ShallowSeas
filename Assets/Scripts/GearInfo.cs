using System;
using System.Collections.Generic;

public enum GearType
{
    None,
    BigNet,
    SmallNet,
    SingleLine
}

public class GearInfo
{
    public GearType m_type;
    public string m_gearName;
    public float m_castDuration;
    public int m_maxCatch;
    public int[] m_catchMultiplier = new int[] { 1, 1, 1 };

    private GearInfo(GearType type, string name, float castDuration, int maxCatch, params int[] catchMultiplier)
    {
        m_type = type;
        m_gearName = name;
        m_castDuration = castDuration;
        m_maxCatch = maxCatch;
        m_catchMultiplier = catchMultiplier;
    }

    static private Dictionary<GearType, GearInfo> s_gearInfo = new Dictionary<GearType, GearInfo>();
    
    static private void addGear(GearType type, string name, float castDuration, int maxCatch, params int[] catchMultiplier)
    {
        s_gearInfo.Add(type, new GearInfo(type, name, castDuration, maxCatch, catchMultiplier));
    }
    
    static GearInfo()
    {
        addGear(GearType.BigNet, "Big Net", 15, 25, 10, 10, 10);
        addGear(GearType.SmallNet, "Small Net", 7.5f, 10, 0, 10, 10);
        addGear(GearType.SingleLine, "Single Line", 1, 1, 10, 10, 10);
    }

    static public GearInfo getInfo(GearType type)
    {
        return s_gearInfo [type];
    }
}

