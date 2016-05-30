using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ShallowNet;
using UnityEngine.SceneManagement;

public class ShopMenu : MonoBehaviour
{
	public ShopItemBuyer m_itemPrefab;
	public RectTransform m_itemsContainer;

	internal GameSettings m_settings;

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

		foreach (var item in m_settings.buyItems)
		{
			var buyer = Util.InstantiatePrefab(m_itemPrefab);
			buyer.transform.SetParent(m_itemsContainer, false);
			buyer.init(item);
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
