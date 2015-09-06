using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CastGearButton : MonoBehaviour
{
    private Button m_button;
    public Image ProgressBarImage;

    public GearType m_gearType;

    public void Start()
    {
        m_button = GetComponent<Button>();

        m_button.onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        MyNetworkPlayer.LocalInstance.CastGear(m_gearType);
    }

    public void Update()
    {
        m_button.interactable = (MyNetworkPlayer.LocalInstance.m_castGear == GearType.None);

        if (MyNetworkPlayer.LocalInstance.m_castGear == m_gearType)
        {
            float start = MyNetworkPlayer.LocalInstance.m_castStartTime;
            float end = MyNetworkPlayer.LocalInstance.m_castEndTime;
            float now = Time.timeSinceLevelLoad;
            ProgressBarImage.fillAmount = (now - start) / (end - start);
        }
        else
        {
            ProgressBarImage.fillAmount = 0;
        }
    }
}
