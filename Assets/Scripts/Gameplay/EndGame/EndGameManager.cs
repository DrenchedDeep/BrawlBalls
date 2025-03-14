using System;
using System.Collections.Generic;
using Gameplay;
using Managers.Network;
using Unity.Netcode;
using System.Linq;
using UnityEngine;

public class EndGameManager : MonoBehaviour
{
    private static readonly int Start = Animator.StringToHash("Start");
    
    [SerializeField] private GameObject container;
    [SerializeField] private Animator animator;
    [SerializeField] private Camera endGameCamera;

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
        NetworkGameManager.Instance.OnGameEnd += OnGameEnd;
    }

    private void OnDisable()
    {
        NetworkGameManager.Instance.OnGameEnd -= OnGameEnd;
    }

    private void OnGameEnd()
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
    }
}
