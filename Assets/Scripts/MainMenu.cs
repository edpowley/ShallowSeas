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

    public void Update()
    {
        bool isConnected = MyNetworkManager.Instance.IsConnected;

        ServerIpInput.interactable = !isConnected;
        ServerPortInput.interactable = !isConnected;
        JoinButton.interactable = !isConnected;

        if (MyNetworkPlayer.LocalInstance != null)
        {
            MyNetworkPlayer.LocalInstance.SetName(PlayerNameInput.text);
        }

        checkPlayerList();
    }

    private void checkPlayerList()
    {
        if (m_playerListEntries.Count != MyNetworkPlayer.Instances.Count())
        {
            rebuildPlayerList();
            return;
        }

        int i = 0;
        foreach (MyNetworkPlayer player in MyNetworkPlayer.Instances)
        {
            if (m_playerListEntries[i].Player != player)
            {
                rebuildPlayerList();
                return;
            }

            i++;
        }
    }

    private void rebuildPlayerList()
    {
        foreach (var entry in m_playerListEntries)
        {
            DestroyObject(entry.gameObject);
        }

        m_playerListEntries.Clear();

        foreach (MyNetworkPlayer player in MyNetworkPlayer.Instances)
        {
            var entry = Util.InstantiatePrefab(PlayerListEntryPrefab);
            entry.transform.SetParent(PlayerListParent, false);
            entry.Player = player;
            m_playerListEntries.Add(entry);

            entry.Update();
        }
    }

    public void OnPlayerNameChanged(string value)
    {
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
    }
}
