using System;
using Gameplay;
using Managers;
using Managers.Local;
using Managers.Network;
using Unity.Cinemachine;
using UnityEngine;

namespace Core.Podium
{
    public class SelectionMenu : MonoBehaviour
    {

        [SerializeField] private CinemachineCamera cam;
        [SerializeField] private PodiumCycleController cycleController;
        
        public static SelectionMenu Instance { get; private set; }


        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            cycleController.onForwardSelected.AddListener(TrySpawnSelectedBall);
            cycleController.onForwardSelected.AddListener(cycleController.DisablePodiumAndCycle);
            
            BeginDisplaying();
        }

        private void Start()
        {
            if (NetworkGameManager.Instance)
            {
                NetworkGameManager.Instance.OnGameBegin += BeginDisplaying;
                NetworkGameManager.Instance.OnGameEnd += EndDisplaying;
                
            }
            else
            {
                Debug.LogWarning("Dude what :/", gameObject);
                BeginDisplaying();
            }

        }

        private void OnDestroy()
        {
            if (NetworkGameManager.Instance)
            {
                NetworkGameManager.Instance.OnGameBegin -= BeginDisplaying;
                NetworkGameManager.Instance.OnGameEnd -= BeginDisplaying;
            }
        }


        public void BeginDisplaying()
        {
            Debug.Log("We are now displaying balls", gameObject);

            cam.enabled = true;
            cycleController.enabled = true;
        }

        public void EndDisplaying()
        {
            Debug.Log("We are not displaying balls anymore", gameObject);
            cam.enabled = false;
            cycleController.enabled = false;
        }
        
        
        public void TrySpawnSelectedBall(int i)
        {
            Debug.Log("Selecting ball: " + PlayerBallInfo.Balls[i].Ball, gameObject);
            if (!NetworkGameManager.Instance.CanRespawn())
            {
                Debug.Log("Player cannot respawn right now!", gameObject);
                return;
            }
            Debug.LogWarning("Add server side check to see if we can still spawn that ball, or if we've already spent it.", gameObject);
            BallHandler.Instance.SpawnBall_ServerRpc(PlayerBallInfo.Balls[i].Ball, PlayerBallInfo.Balls[i].Weapon, PlayerBallInfo.Balls[i].Ability);
            EndDisplaying();
        }
    }
}