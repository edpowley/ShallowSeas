using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenuPlayerListEntry : MonoBehaviour
{
    public string PlayerId;
    public Image Icon;
    public Text PlayerName;

    public void setPlayerInfo(ShallowNet.PlayerInfo info)
    {
        PlayerId = info.Id;
        PlayerName.text = info.Name;
        //Icon.color = Player.PlayerColour;
    }
}
