using System;
using Cysharp.Threading.Tasks;
using Managers.Local;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Loading.LoadingCheckpoints
{
    
    [RequireComponent(typeof(PlayerInput)), DefaultExecutionOrder(-100)]
    public class GameDataLoadingCheckpoint : MonoBehaviour, ILoadingCheckpoint
    {
        private bool _isLoaded;
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
            Debug.Log("Beginning loading checkpoint", gameObject);
            
            await SaveManager.LoadPlayer(_localPlayer);

            Debug.Log("Loading complete.");
            
            _isLoaded = true;
        }

        public bool IsCompleted()
        {
            return _isLoaded;
        }
    }
    

}
