using UnityEngine;
using System.Collections;
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

    public string m_serverHost;
    public int m_serverPort;

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
        MyNetworkManager.Instance.JoinServer(m_serverHost, m_serverPort, PlayerNameInput.text);
    }
}
