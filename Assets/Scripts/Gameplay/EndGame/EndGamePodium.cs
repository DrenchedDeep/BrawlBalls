using TMPro;
using UnityEngine;

public class EndGamePodium : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;


    //need some way to get either their last ball played or their most played ball...
    public void SetupWithPlayer(string playerName)
    {
        playerNameText.text = playerName;
    }
}
