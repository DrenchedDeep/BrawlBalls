using System;
using Cysharp.Threading.Tasks;
using Gameplay.Map;
using Managers.Local;
using Managers.Network;
using TMPro;
using UnityEngine;

namespace Gameplay.Spectating
{
    public class SpectatingManager : MonoBehaviour
    {
        private PlayerController _localPlayerController;
        [SerializeField] private TextMeshProUGUI currentPlayerName;

        [SerializeField] private Canvas spectatingUI;
    
        private int _currentIndex;
        private bool _checkForBall;
        private float _checkForTime;

        private void Awake()
        {
            _localPlayerController = transform.root.GetComponent<PlayerController>();
        }

        public void StartSpectating()
        {
            spectatingUI.enabled = true;
            SpectateNextPlayer(0);
        }

        public void StopSpectating()
        {
            spectatingUI.enabled = false;
        }
    
        public void SpectateNextPlayer(int index)
        {
            if (spectatingUI.enabled)
            {
                spectatingUI.enabled = true;
            }
            
            Debug.LogWarning("This should not use FindObjectsByType, we should instead be updating an array whenever a new BallPlayer is spawned / destroyed. This function currently will lag by just mashing left or right");

            //IM LAZY ASF BABY LETS GOOO!!
            BallPlayer[] allBalls = FindObjectsByType<BallPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            if (allBalls.Length <= 0)
            {
                _checkForBall = true;
                return;
            }
            
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
            _localPlayerController.SetBall(allBalls[_currentIndex]);

            string playerName =
                NetworkGameManager.Instance.GetPlayerName(allBalls[_currentIndex].NetworkObject.OwnerClientId, allBalls[_currentIndex].ChildID.Value);
        
            Debug.Log("I am now spectating: " +
                      playerName);
            
            currentPlayerName.text = playerName;
        }


        private void Update()
        {
            if (_checkForBall)
            {
                _checkForTime += Time.deltaTime;

                if (_checkForTime >= 1f)
                {
                    BallPlayer[] allBalls = FindObjectsByType<BallPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

                    if (allBalls.Length > 0)
                    {
                        SpectateNextPlayer(0);
                        _checkForBall = false;
                        _checkForTime = 0;
                    }
                }
            }
        }

        private void OnCurrentSpectatingPlayerDied(ulong killer, int childID)
        {
            SpectateNextPlayer(1);
        }
    }
}
