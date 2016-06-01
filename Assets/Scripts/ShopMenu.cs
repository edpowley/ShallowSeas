using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using ShallowNet;
using UnityEngine.SceneManagement;

public class ShopMenu : MonoBehaviour
{
	public ShopItemBuyer m_itemPrefab;
	public RectTransform m_itemsContainer;
	public Text m_moneyDisplay, m_doneButtonLabel, m_statsDisplay;
	public CanvasGroup m_canvasGroup;

	internal GameSettings m_settings;
	internal int m_money;
	private List<ShopItemBuyer> m_itemBuyers = new List<ShopItemBuyer>();

	public void Awake()
	{
		// If the network manager isn't running, go back to the main menu
		// (shouldn't happen in game, but is useful for testing in the Unity editor)
		if (MyNetworkManager.Instance == null)
			StartCoroutine(returnToMainMenuAfterDelay(0.1f));
	}

	private IEnumerator returnToMainMenuAfterDelay(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		SceneManager.LoadScene((int)Level.MainMenu);
	}

	// Use this for initialization
	void Start()
	{
		m_settings = MyNetworkManager.Instance.m_welcomeMsg.Settings;
		m_money = MyNetworkManager.Instance.m_startShopMsg.Money;

		foreach (var item in m_settings.buyItems)
		{
			var buyer = Util.InstantiatePrefab(m_itemPrefab);
			buyer.transform.SetParent(m_itemsContainer, false);
			buyer.init(this, item);
			m_itemBuyers.Add(buyer);
		}

		updateMoneyDisplay();

		m_statsDisplay.text = MyNetworkManager.Instance.m_startShopMsg.Stats;

		var client = MyNetworkManager.Instance.m_client;
		client.addMessageHandler<InformBuy>(this, handleInformBuy);
	}

	public void OnDestroy()
	{
		if (MyNetworkManager.Instance != null && MyNetworkManager.Instance.m_client != null)
			MyNetworkManager.Instance.m_client.removeMessageHandlers(this);
	}

	void updateMoneyDisplay()
	{
		m_moneyDisplay.text = string.Format("Money left: ${0}", m_money);

		foreach(var buyer in m_itemBuyers)
		{
			buyer.updateValues();
		}
	}

	// Update is called once per frame
	void Update()
	{

	}

	void handleInformBuy(ClientWrapper client, InformBuy msg)
	{
		if (msg.PlayerId == MyNetworkManager.Instance.LocalPlayerId)
		{
			m_money = msg.PlayerMoney;
			updateMoneyDisplay();
			m_canvasGroup.interactable = true;
		}

		foreach (var buyer in m_itemBuyers)
		{
			if (buyer.m_itemInfo.name == msg.Item)
			{
				buyer.CurrentSpend = msg.ItemSpend;
			}
		}
	}

	public void onDoneClicked()
	{
		MyNetworkManager.Instance.m_client.sendMessage(new FinishedShopping());
		m_canvasGroup.interactable = false;
		m_doneButtonLabel.text = "Waiting for other players";
	}
}
