using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CastGearButton : MonoBehaviour
{
    private Button m_button;
    public Image ProgressBarImage;

    public string GearName;
    public float CastDuration;
    public int MaxCatch;
    public int[] CatchMultiplier = new int[] { 1, 1, 1 };

    public void Start()
    {
        m_button = GetComponent<Button>();

        m_button.onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        MyNetworkPlayer.LocalInstance.CastGear(this);
    }

    public void Update()
    {
        m_button.interactable = (MyNetworkPlayer.LocalInstance.m_castGear == null);

        if (MyNetworkPlayer.LocalInstance.m_castGear == this.GearName)
        {
            ProgressBarImage.fillAmount = MyNetworkPlayer.LocalInstance.m_castProgress;
        }
        else
        {
            ProgressBarImage.fillAmount = 0;
        }
    }
}
