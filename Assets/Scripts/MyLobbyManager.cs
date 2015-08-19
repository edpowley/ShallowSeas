using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MyLobbyManager : NetworkLobbyManager
{
    public void HostGame()
    {
        StartHost();
    }

    public void JoinGame()
    {
        StartClient();
    }
    
    public void SetServerAddress(string server)
    {
        Debug.Log(server);
    }


}
