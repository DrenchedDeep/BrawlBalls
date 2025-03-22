using System;
using System.Collections.Generic;
using Core.Podium;
using Gameplay.EndGame;
using Managers.Local;
using Managers.Network;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Gameplay
{
    public class GameStateManager : MonoBehaviour
    {
        [Serializable]
        public struct GameStateSettings
        {
            public GameState gameState;
            public UnityEvent enableStateEvent;
            public UnityEvent disableStateEvent;
        }

        public static GameStateManager Instance { get; private set; }

        [SerializeField] private GameStateSettings[] gameStateSettings = Array.Empty<GameStateSettings>();
        

        private int _currentGameStateSettings;

        public GameStateSettings CurrentGameStateSettings => gameStateSettings[_currentGameStateSettings];

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
        //    NetworkGameManager.Instance.OnGameStateUpdated += OnGameStateChanged;
        }

        private void OnDisable()
        {
     //       NetworkGameManager.Instance.OnGameStateUpdated -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState gameState)
        {
            gameStateSettings[_currentGameStateSettings].disableStateEvent?.Invoke();
            _currentGameStateSettings = GetIndexFromGameState(gameState);
            CurrentGameStateSettings.enableStateEvent?.Invoke();
        }

        private int GetIndexFromGameState(GameState state)
        {
            for (int i = 0; i < gameStateSettings.Length; i++)
            {
                if (state == gameStateSettings[i].gameState)
                {
                    return i;
                }
            }

            return -1;
        }
        

    }
}
