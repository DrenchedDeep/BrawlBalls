using Gameplay;
using Managers.Local;
using Managers.Network;
using TMPro;
using UnityEngine;

public class SpectatingManager : MonoBehaviour
{
    [SerializeField] private LocalPlayerController localPlayerController;
    [SerializeField] private TextMeshProUGUI currentPlayerName;

    [SerializeField] private GameObject spectatingUI;
    
    private int _currentIndex;

    public void StartSpectating()
    {
        spectatingUI.SetActive(true);
        SpectateNextPlayer(0);
    }
    
    public void SpectateNextPlayer(int index)
    {
        //IM LAZY ASF BABY LETS GOOO!!
        Ball[] allBalls = FindObjectsByType<Ball>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        _currentIndex += index % allBalls.Length;

        if (allBalls[_currentIndex])
        {
            localPlayerController.SetBall(allBalls[_currentIndex]);

            string playerName =
                NetworkGameManager.Instance.GetPlayerName(allBalls[_currentIndex].NetworkObject.OwnerClientId);
            Debug.Log("I am now spectating: " +
                      playerName);
            
            currentPlayerName.text = playerName;
        }
    }
}
