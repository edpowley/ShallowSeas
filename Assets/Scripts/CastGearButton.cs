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
        if (GameManager.Instance.m_localPlayerBoat != null)
            GameManager.Instance.m_localPlayerBoat.CastGear(this);
    }

    public void Update()
    {
        Boat playerBoat = GameManager.Instance.m_localPlayerBoat;

        if (playerBoat != null)
        {
            m_button.interactable = (playerBoat.m_castGear == null);

            if (playerBoat.m_castGear == this.GearName)
            {
                ProgressBarImage.fillAmount = playerBoat.m_castProgress;
            }
            else
            {
                ProgressBarImage.fillAmount = 0;
            }
        }
        else
        {
            m_button.interactable = false;
            ProgressBarImage.fillAmount = 0;
        }
    }
}
