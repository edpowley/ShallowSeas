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
        GearInfo gear = GearInfo.getInfo(m_gearType);

        var msg = new ShallowNet.RequestCastGear();
        var boatPos = GameManager.Instance.LocalPlayerBoat.transform.position;
        msg.Position = new ShallowNet.SNVector2(boatPos.x, boatPos.z);
        msg.GearName = m_gearType.ToString();
        msg.CastDuration = gear.m_castDuration;
        msg.CatchMultipliers = gear.m_catchMultiplier;
        msg.MaxCatch = gear.m_maxCatch;

        MyNetworkManager.Instance.m_client.sendMessage(msg);
    }

    public void Update()
    {
        if (GameManager.Instance.LocalPlayerBoat != null)
        {
            m_button.interactable = (GameManager.Instance.LocalPlayerBoat.m_castGear == GearType.None);

            if (GameManager.Instance.LocalPlayerBoat.m_castGear == m_gearType)
            {
                float start = GameManager.Instance.LocalPlayerBoat.m_castStartTime;
                float end = GameManager.Instance.LocalPlayerBoat.m_castEndTime;
                float now = GameManager.Instance.CurrentTime;
                ProgressBarImage.fillAmount = (now - start) / (end - start);
            } else
            {
                ProgressBarImage.fillAmount = 0;
            }
        } else
        {
            m_button.interactable = false;
        }
    }
}
