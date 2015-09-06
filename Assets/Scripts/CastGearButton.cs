using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CastGearButton : MonoBehaviour
{
    private Button m_button;
    public Image ProgressBarImage;

    public GearInfo m_gearInfo;

    public void Start()
    {
        m_button = GetComponent<Button>();

        m_button.onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        MyNetworkPlayer.LocalInstance.CastGear(m_gearInfo);
    }

    public void Update()
    {
        m_button.interactable = (MyNetworkPlayer.LocalInstance.m_castGear == null);

        if (MyNetworkPlayer.LocalInstance.m_castGear == m_gearInfo.m_gearName)
        {
            ProgressBarImage.fillAmount = MyNetworkPlayer.LocalInstance.m_castProgress;
        }
        else
        {
            ProgressBarImage.fillAmount = 0;
        }
    }
}
