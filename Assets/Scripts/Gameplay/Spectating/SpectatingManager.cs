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

    public void StopSpectating()
    {
        spectatingUI.SetActive(false);
    }
    
    public void SpectateNextPlayer(int index)
    {

        //IM LAZY ASF BABY LETS GOOO!!
        BallPlayer[] allBalls = FindObjectsByType<BallPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (allBalls[_currentIndex])
        {
            allBalls[_currentIndex].OnDestroyed -= OnCurrentSpectatingPlayerDied;
        }
        
        _currentIndex += index;
        _currentIndex %= allBalls.Length;

        if (!allBalls[_currentIndex])
        {
            _currentIndex = 0;
        }

        allBalls[_currentIndex].OnDestroyed += OnCurrentSpectatingPlayerDied;
        localPlayerController.SetBall(allBalls[_currentIndex].GetBall);

        string playerName =
            NetworkGameManager.Instance.GetPlayerName(allBalls[_currentIndex].NetworkObject.OwnerClientId);
        
        Debug.Log("I am now spectating: " +
                  playerName);
            
        currentPlayerName.text = playerName;
        

    }

    private void OnCurrentSpectatingPlayerDied(ulong killer)
    {
        SpectateNextPlayer(1);
    }
}
