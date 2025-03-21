using System;
using System.Net;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Loading.LoadingCheckpoints
{
    public class AuthenticationLoadingCheckpoint : MonoBehaviour, ILoadingCheckpoint
    {
        public Action OnComplete { get; set; }
        public Action OnFailed { get; set; }
        
        private static bool _initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInit()
        {
            _initialized = false;
        }

        public async UniTask Execute()
        {
            await UnityServices.InitializeAsync();
            _initialized = true;
            
            AuthenticationService.Instance.SignedIn += OnComplete.Invoke;

            try
            {
                
                Debug.LogWarning("Implement Profile Switching -- Probably need to revisit for local conncections");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (ServicesInitializationException)
            {
                Debug.LogError("Authentication Service Failed");
                OnFailed?.Invoke();
                return;
            }
        }

        public bool IsCompleted()
        {
            return _initialized && AuthenticationService.Instance.IsSignedIn;
        }
    }
}
