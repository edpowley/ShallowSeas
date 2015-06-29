using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CastGearButton : MonoBehaviour
{
    private Button m_button;
    public Image ProgressBarImage;
    public GearItem Gear;

    public void Start()
    {
        m_button = GetComponent<Button>();

        m_button.onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        GameManager.Instance.PlayerBoat.CastGear(this.Gear);
    }

    public void Update()
    {
        Boat playerBoat = GameManager.Instance.PlayerBoat;

        m_button.interactable = (playerBoat.m_currentCastGear == null);

        if (playerBoat.m_currentCastGear == this.Gear)
        {
            ProgressBarImage.fillAmount = playerBoat.m_currentCastGear.Progress;
        }
        else
        {
            ProgressBarImage.fillAmount = 0;
        }
    }
}
