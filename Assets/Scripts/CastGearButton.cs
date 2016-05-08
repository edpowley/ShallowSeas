using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CastGearButton : MonoBehaviour
{
    private Button m_button;
    public Image m_progressBarImage;

    public void Start()
    {
        m_button = GetComponent<Button>();
        m_button.onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        GearSelector gearSelector = FindObjectOfType<GearSelector>();
        string gearType = gearSelector.SelectedGearType;

        var msg = new ShallowNet.RequestCastGear();
        var boatPos = GameManager.Instance.LocalPlayerBoat.transform.position;
        msg.Position = new ShallowNet.SNVector2(boatPos.x, boatPos.z);
        msg.GearName = gearType.ToString();

        MyNetworkManager.Instance.m_client.sendMessage(msg);
    }

    public void Update()
    {
        if (GameManager.Instance.LocalPlayerBoat != null)
        {
            m_button.interactable = (GameManager.Instance.LocalPlayerBoat.m_castGear == null);

            if (GameManager.Instance.LocalPlayerBoat.m_castGear != null)
            {
                float start = GameManager.Instance.LocalPlayerBoat.m_castStartTime;
                float end = GameManager.Instance.LocalPlayerBoat.m_castEndTime;
                float now = GameManager.Instance.CurrentTime;
                m_progressBarImage.fillAmount = (now - start) / (end - start);
            }
            else
            {
                m_progressBarImage.fillAmount = 0;
            }
        }
        else
        {
            m_button.interactable = false;
        }
    }
}
