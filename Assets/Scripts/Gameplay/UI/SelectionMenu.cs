using System.Collections.Generic;
using Managers;
using Managers.Local;
using Managers.Network;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.UI
{
    public class SelectionMenu : MonoBehaviour
    {

        private int _currentBallSelected;
        private readonly List<BallPlayer> _balls = new();
        private bool _init = false;

        private void Start()
        {
            _init = true;
            //Create all the balls on the podium    
            _balls.AddRange(BallHandler.Instance.SpawnShowcaseBalls());
            foreach (BallPlayer b in _balls)
            {
                b.gameObject.SetActive(true);
            }
        }

        private void OnEnable()
        {
            if (!_init) return;
            
            if (_balls.Count == 0)
            {
                SceneManager.LoadScene(0);
                return;
            }
            
            foreach (BallPlayer b in _balls)
            {
                b.gameObject.SetActive(true);
            }
            
     
        }

        private void OnDisable()
        {
            foreach (BallPlayer b in _balls)
            {
                b.gameObject.SetActive(false);
            }
        }

        public void SelectBall(int i)
        {

            
            _currentBallSelected = i;
            TrySpawn();
            print("Selecting ball: " + PlayerBallInfo.Balls[i].Ball);
            //inGameUI.SetActive(true);
        }

        public void TrySpawn()
        {
            if (!NetworkGameManager.Instance.CanRespawn())
            {
                Debug.Log("Player cannot respawn right now!");
                return;
            }
            Debug.LogWarning("Add server side check to see if we can still spawn that ball, or if we've already spent it.");
            BallHandler.Instance.SpawnBall_ServerRpc(PlayerBallInfo.Balls[_currentBallSelected].Ball, PlayerBallInfo.Balls[_currentBallSelected].Weapon, PlayerBallInfo.Balls[_currentBallSelected].Ability);
            gameObject.SetActive(false);
            _balls.RemoveAt(_currentBallSelected);
        }
    }
}