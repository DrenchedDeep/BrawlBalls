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
                
                Debug.LogWarning("Implement Profile Switching");
#if UNITY_EDITOR
                // ParrelSync should only be used within the Unity Editor so you should use the UNITY_EDITOR define
                if (ParrelSync.ClonesManager.IsClone())
                {
                    // When using a ParrelSync clone, switch to a different authentication profile to force the clone
                    // to sign in as a different anonymous user account.
                    string customArgument = ParrelSync.ClonesManager.GetArgument();
                    AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
                }
#endif
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
