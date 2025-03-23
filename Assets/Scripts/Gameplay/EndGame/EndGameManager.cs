using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers.Network;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.EndGame
{
    public class EndGameManager : MonoBehaviour
    {
        private static readonly int Start = Animator.StringToHash("Start");
    
        [SerializeField] private GameObject container;
        [SerializeField] private Animator animator;
        [SerializeField] private CinemachineCamera endGameCamera;

        [Space] 
    
        [SerializeField] private EndGamePodium firstPlacePodium;
        [SerializeField] private EndGamePodium secondPlacePodium;
        [SerializeField] private EndGamePodium thirdPlace;

        private void Awake()
        {
            endGameCamera.enabled = false;
        }

        private void OnEnable()
        {
            NetworkGameManager.Instance.OnGameStateUpdated += OnGameStateChanged;
        }
        
        private void OnDisable()
        { 
            NetworkGameManager.Instance.OnGameStateUpdated -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.EndingGameCinematic)
            {
                OnGameEnd();
            }
        }

        public void OnGameEnd()
        {
            List<BallPlayerInfo> players = new List<BallPlayerInfo>();

            foreach (BallPlayerInfo player in NetworkGameManager.Instance.Players)
            {
                players.Add(player);
            }
        
            List<BallPlayerInfo> topPlayers = players
                .OrderByDescending(p => p.Score) 
                .Take(3) 
                .ToList();
        
            firstPlacePodium.SetupWithPlayer(topPlayers[0].Username.ToString());

            if (players.Count >= 2)
            {
                secondPlacePodium.SetupWithPlayer(topPlayers[1].Username.ToString());
            }
            else
            {
                secondPlacePodium.gameObject.SetActive(false);
            }

            if (players.Count >= 3)
            {
                thirdPlace.SetupWithPlayer(topPlayers[2].Username.ToString());
            }
            else
            {
                thirdPlace.gameObject.SetActive(false);
            }

            endGameCamera.enabled = true;
            animator.SetTrigger(Start);
            StartCoroutine(LeaveGame());
        }

        private IEnumerator LeaveGame()
        {
            yield return new WaitForSeconds(20f);
            
            NetworkGameManager.Instance.ReturnToMainMenu();
            
        }
    }
    
}
