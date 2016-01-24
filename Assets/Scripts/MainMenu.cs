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
        processCommandLineArguments();

        ServerIpInput.text = m_serverHost;
        ServerPortInput.text = m_serverPort.ToString();
    }

    private void processCommandLineArguments()
    {
        bool autoStart = false;

        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            string nextArg = (i < args.Length - 1) ? args[i + 1] : "";
            switch (args[i])
            {
                case "-name":
                    PlayerNameInput.text = nextArg;
                    break;

                case "-host":
                    m_serverHost = nextArg;
                    break;

                case "-port":
                    int.TryParse(nextArg, out m_serverPort);
                    break;

                case "-join":
                    autoStart = true;
                    break;
            }
        }

        if (autoStart)
            OnJoinClicked();
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
