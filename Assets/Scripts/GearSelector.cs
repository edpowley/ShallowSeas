using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GearSelector : MonoBehaviour
{
    private Dropdown m_dropdown;

    private List<GearType> m_gearTypes;

    internal GearType SelectedGearType
    {
        get { return m_gearTypes[m_dropdown.value]; }
        set { m_dropdown.value = m_gearTypes.IndexOf(value); }
    }

    void Start()
    {
        m_dropdown = GetComponent<Dropdown>();

        m_gearTypes = new List<GearType>(System.Enum.GetValues(typeof(GearType)).Cast<GearType>());
        m_gearTypes.Remove(GearType.None);

        m_dropdown.ClearOptions();
        List<string> gearNames = new List<string>(from gear in m_gearTypes select GearInfo.getInfo(gear).m_gearName);
        m_dropdown.AddOptions(gearNames);
        m_dropdown.value = 0;
    }

    public void Update()
    {
        if (GameManager.Instance.LocalPlayerBoat != null)
            m_dropdown.interactable = (GameManager.Instance.LocalPlayerBoat.m_castGear == GearType.None);
    }
}
