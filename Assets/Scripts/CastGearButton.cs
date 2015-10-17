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
        //GameManager.Instance.LocalPlayerBoat.CastGear(m_gearType);
    }

    public void Update()
    {
        /*m_button.interactable = (GameManager.Instance.LocalPlayerBoat.m_castGear == GearType.None);

        if (GameManager.Instance.LocalPlayerBoat.m_castGear == m_gearType)
        {
            float start = GameManager.Instance.LocalPlayerBoat.m_castStartTime;
            float end = GameManager.Instance.LocalPlayerBoat.m_castEndTime;
            float now = Time.timeSinceLevelLoad;
            ProgressBarImage.fillAmount = (now - start) / (end - start);
        }
        else
        {
            ProgressBarImage.fillAmount = 0;
        }*/
    }
}
