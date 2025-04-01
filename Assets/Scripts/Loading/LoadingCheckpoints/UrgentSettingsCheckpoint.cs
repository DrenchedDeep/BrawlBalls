using System;
using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;

namespace Loading.LoadingCheckpoints
{
    public class UrgentSettingsCheckpoint : MonoBehaviour, ILoadingCheckpoint
    {

        public Action OnComplete { get; set; }
        public Action OnFailed { get; set; }

        private static bool _isComplete;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void RuntimeInit()
        {
            _isComplete = false;
        }

        public UniTask Execute()
        {
            
            try
            {
                RuntimeManager.GetBus("Bus:/").setVolume(PlayerPrefs.GetFloat("Audio_Bus:/", 0.5f));
                RuntimeManager.GetBus("Bus:/Music").setVolume(PlayerPrefs.GetFloat("Audio_Bus:/Music", 0.5f));
                RuntimeManager.GetBus("Bus:/SFXs").setVolume(PlayerPrefs.GetFloat("Audio_Bus:/SFXs", 0.5f));
                RuntimeManager.GetBus("Bus:/Ambience").setVolume(PlayerPrefs.GetFloat("Audio_Bus:/Ambience", 0.5f));
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load FMOD: " + e);
            }
            finally
            {
                _isComplete = true;
            }
            return UniTask.CompletedTask;

        }

        public bool IsCompleted()
        {
            return _isComplete;
        }
    }
}
