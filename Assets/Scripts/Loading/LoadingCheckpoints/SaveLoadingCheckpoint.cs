using System;
using Cysharp.Threading.Tasks;
using Managers.Local;
using Managers.Network;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Loading.LoadingCheckpoints
{
    [RequireComponent(typeof(PlayerInput)), DefaultExecutionOrder(-99)]
    public class SaveLoadingCheckpoint : MonoBehaviour, ILoadingCheckpoint
    {
        public Action OnComplete { get; set; }
        public Action OnFailed { get; set; }

        private PlayerInput _localPlayer;

        private void Awake()
        {
            _localPlayer = GetComponent<PlayerInput>();
            LoadingController.Instance.RegisterLoadingComponent(this);
            LoadingController.Instance.BeginLoading();
        }

        private void Start()
        {
            LobbySystemManager.Instance.OnGameStarting += OnInstanceOnOnGameStarting;
        }

        private void OnDestroy()
        {
            if(LobbySystemManager.Instance) LobbySystemManager.Instance.OnGameStarting -= OnInstanceOnOnGameStarting;
        }

        void OnInstanceOnOnGameStarting() => _ = Execute();


        public async UniTask Execute()
        {
            try
            {
                Debug.Log("Beginning Save");
                if (!SaveManager.TryGetPlayerData(_localPlayer, out SaveManager.PlayerData x))
                {
                    Debug.LogError("The player data key was missing... Generating a default one... We should have generated a default one with the GameDataLoadingCheckpoint, but we haven't?");
                }

                await x.SaveData();
                
                Debug.Log("Saving Complete");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                OnFailed?.Invoke();
            }
            OnComplete?.Invoke();
        }

        public bool IsCompleted()
        {
            return SaveManager.TryGetPlayerData(_localPlayer, out SaveManager.PlayerData x) && !x.HasChanges();
        }
    }
}
