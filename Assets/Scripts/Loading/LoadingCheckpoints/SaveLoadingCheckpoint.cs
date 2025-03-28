using System;
using Cysharp.Threading.Tasks;
using Managers.Local;
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
            _ = LoadingController.Instance.BeginLoading();
        }


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
