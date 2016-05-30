using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using ShallowNet;

public class ShopItemBuyer : MonoBehaviour
{
	public Text m_nameText, m_amountText;
	public Slider m_slider;
	public Button m_buyButton;
	public Image m_progressBar;

	private GameSettings.BuyInfo m_itemInfo;
	private int m_currentSpend;

	// Use this for initialization
	void Start()
	{

	}

	internal void init(GameSettings.BuyInfo itemInfo)
	{
		m_itemInfo = itemInfo;

		m_currentSpend = 0;
		switch (itemInfo.category)
		{
			case GameSettings.BuyCategory.Self:
				MyNetworkManager.Instance.m_startShopMsg.PlayerSpend.TryGetValue(itemInfo.name, out m_currentSpend);
				break;

			case GameSettings.BuyCategory.Group:
				MyNetworkManager.Instance.m_startShopMsg.GroupSpend.TryGetValue(itemInfo.name, out m_currentSpend);
				break;
		}

		m_nameText.text = string.Format("[{0}] {1}", itemInfo.category, itemInfo.name);

		m_progressBar.fillAmount = (float)m_currentSpend / itemInfo.price;

		m_slider.minValue = 0;
		m_slider.maxValue = itemInfo.price - m_currentSpend;
		m_slider.value = 0;

		sliderValueChanged();
	}
	
	public void sliderValueChanged()
	{
		if (m_slider.value > 0)
		{
			m_buyButton.interactable = true;
			m_amountText.text = string.Format("${0} + ${1} / ${2}", m_currentSpend, m_slider.value, m_itemInfo.price);
		}
		else
		{
			m_buyButton.interactable = false;
			m_amountText.text = string.Format("${0} / ${1}", m_currentSpend, m_itemInfo.price);
		}
	}

	public void buyClicked()
	{

	}

}
