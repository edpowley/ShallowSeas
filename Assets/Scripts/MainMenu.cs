using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using ShallowNet;

public class MainMenu : MonoBehaviour
{
    public InputField PlayerNameInput, ServerIpInput, ServerPortInput;
    public Button JoinButton;
    public MainMenuPlayerListEntry PlayerListEntryPrefab;
    public RectTransform PlayerListParent;

    public string m_serverHost;
    public int m_serverPort;

    private List<MainMenuPlayerListEntry> m_playerListEntries = new List<MainMenuPlayerListEntry>();

    public void Start()
    {
        ServerIpInput.text = m_serverHost;
        ServerPortInput.text = m_serverPort.ToString();
    }

    public void OnDestroy()
    {
        if (MyNetworkManager.Instance != null && MyNetworkManager.Instance.m_client != null)
            MyNetworkManager.Instance.m_client.removeMessageHandlers(this);
    }

    public void Update()
    {
        bool isConnected = MyNetworkManager.Instance.IsConnected;

        ServerIpInput.interactable = !isConnected;
        ServerPortInput.interactable = !isConnected;
        JoinButton.interactable = !isConnected;
    }

    private void handlePlayerList(ClientWrapper client, SetPlayerList msg)
    {
        foreach (var entry in m_playerListEntries)
        {
            DestroyObject(entry.gameObject);
        }
        
        m_playerListEntries.Clear();
        
        foreach(PlayerInfo info in msg.Players)
        {
            var entry = Util.InstantiatePrefab(PlayerListEntryPrefab);
            entry.transform.SetParent(PlayerListParent, false);
            entry.setPlayerInfo(info);
            m_playerListEntries.Add(entry);
        }
    }
    
    private void handlePlayerInfo(ClientWrapper client, SetPlayerInfo msg)
    {
        var entry = m_playerListEntries.SingleOrDefault(e => e.PlayerId == msg.Player.Id);
        if (entry != null)
        {
            entry.setPlayerInfo(msg.Player);
        }
        else
        {
            Debug.LogFormat("No entry for id {0}", msg.Player.Id);
        }
    }
    
    public void OnPlayerNameChanged(string value)
    {
        if (MyNetworkManager.Instance.IsConnected)
        {
            MyNetworkManager.Instance.LocalPlayer.Name = value;

            MyNetworkManager.Instance.m_client.sendMessage(new SetPlayerName() { NewName = value });

            var entry = m_playerListEntries.SingleOrDefault(e => e.PlayerId == MyNetworkManager.Instance.LocalPlayerId);
            if (entry != null)
                entry.PlayerName.text = value;
        }
    }
    
    public void OnServerIpChanged(string value)
    {
        m_serverHost = value;
    }
    
    public void OnServerPortChanged(string value)
    {
        m_serverPort = int.Parse(value);
    }

    public void OnJoinClicked()
    {
        MyNetworkManager.Instance.JoinServer(m_serverHost, m_serverPort);

        MyNetworkManager.Instance.m_client.addMessageHandler<SetPlayerList>(this, handlePlayerList);
        MyNetworkManager.Instance.m_client.addMessageHandler<SetPlayerInfo>(this, handlePlayerInfo);

        MyNetworkManager.Instance.m_client.sendMessage(new PlayerJoinRequest() { PlayerName = PlayerNameInput.text });
    }
}
