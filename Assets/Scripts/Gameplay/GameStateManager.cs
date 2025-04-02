using System;
using Managers.Network;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay
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


        [SerializeField] private GameStateSettings[] gameStateSettings = Array.Empty<GameStateSettings>();
        

        private int _currentGameStateSettings;

        public GameStateSettings CurrentGameStateSettings => gameStateSettings[_currentGameStateSettings];
        
        private void OnEnable()
        {
            NetworkGameManager.Instance.OnGameStateUpdated += OnGameStateChanged;
        }

        private void OnDisable()
        {
           NetworkGameManager.Instance.OnGameStateUpdated -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState gameState)
        {
            Debug.Log("GAME STATE IS NOW: " + gameState);
            if (_currentGameStateSettings >= 0 && _currentGameStateSettings < gameStateSettings.Length)
            {
                gameStateSettings[_currentGameStateSettings].disableStateEvent?.Invoke();
            }

            _currentGameStateSettings = GetIndexFromGameState(gameState);

            if (_currentGameStateSettings == -1)
            {
                return;
            }
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
