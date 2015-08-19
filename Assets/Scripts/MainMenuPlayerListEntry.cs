using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenuPlayerListEntry : MonoBehaviour
{
    public MyNetworkPlayer Player;
    public Image Icon;
    public Text PlayerName;

    public void Update()
    {
        PlayerName.text = Player.PlayerName;
        Icon.color = Player.PlayerColour;
    }
}
