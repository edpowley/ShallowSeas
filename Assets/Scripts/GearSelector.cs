using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GearSelector : MonoBehaviour
{
    private Dropdown m_dropdown;

    private List<string> m_gearTypes;

    internal string SelectedGearType
    {
        get { return m_gearTypes[m_dropdown.value]; }
        set { m_dropdown.value = m_gearTypes.IndexOf(value); }
    }

    void Start()
    {
        m_dropdown = GetComponent<Dropdown>();

        m_gearTypes = new List<string>(from gear in GameManager.Instance.m_settings.gear select gear.name);

        m_dropdown.ClearOptions();
        m_dropdown.AddOptions(m_gearTypes);
        m_dropdown.value = 0;
    }

    public void Update()
    {
        if (GameManager.Instance.LocalPlayerBoat != null)
            m_dropdown.interactable = (GameManager.Instance.LocalPlayerBoat.m_castGear == null);
    }
}
