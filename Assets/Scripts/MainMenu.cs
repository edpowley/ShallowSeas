using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    public InputField PlayerNameInput, ServerIpInput, ServerPortInput;
    public Button HostButton, JoinButton, StartButton, LeaveButton;
    public MainMenuPlayerListEntry PlayerListEntryPrefab;
    public RectTransform PlayerListParent;

    private List<MainMenuPlayerListEntry> m_playerListEntries = new List<MainMenuPlayerListEntry>();

    public void Start()
    {
        ServerIpInput.text = MyNetworkManager.Instance.networkAddress;
        ServerPortInput.text = MyNetworkManager.Instance.networkPort.ToString();
    }

    public void Update()
    {
        bool isConnected = MyNetworkManager.Instance.isNetworkActive;

        ServerIpInput.interactable = !isConnected;
        ServerPortInput.interactable = !isConnected;
        HostButton.interactable = !isConnected;
        JoinButton.interactable = !isConnected;
        StartButton.interactable = isConnected && MyNetworkManager.Instance.isServer;
        LeaveButton.interactable = isConnected;

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
        MyNetworkManager.Instance.networkAddress = value;
    }
    
    public void OnServerPortChanged(string value)
    {
        MyNetworkManager.Instance.networkPort = int.Parse(value);
    }

    public void OnHostClicked()
    {
        MyNetworkManager.Instance.StartHost();
    }
    
    public void OnJoinClicked()
    {
        MyNetworkManager.Instance.StartClient();
    }

    public void OnStartClicked()
    {
        Debug.Log("OnStartClicked");
        MyNetworkManager.Instance.ChangeLevel(Level.MainGame);
    }
    
    public void OnLeaveClicked()
    {
        MyNetworkManager.Instance.Stop();
    }
}
