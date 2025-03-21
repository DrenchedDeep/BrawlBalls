using Cysharp.Threading.Tasks;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Managers.Local
{
    public class MusicManager : MonoBehaviour
    {
        // FMOD global parameter names (ensure these match your FMOD project)
        private const string ActionParam = "ActionVolume";
        private const string BaseAParam = "BaseAVolume";
        private const string BaseBParam = "BaseBVolume";
        private const string MasterParam = "MasterVolume";
        
        //Replace A and B with just a simple "Transition"
        //Some way to cut all and fade back in?
        //
        
        
        private bool _useAltA = true; // Track which alternating sound is active

        public static MusicManager Instance { get; private set; }
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Set initial volumes using FMOD global parameters (values are linear 0–1)
            SetVolume(ActionParam, 0f);   // Mute initially
            SetVolume(BaseAParam, 0f);    // Mute initially
            SetVolume(MasterParam, 0f);   // Mute initially
            SetVolume(BaseBParam, 0f);    // AltB muted

            const float initialFadeDuration = 3f;
            
            // Fade in BaseA and Master global parameters
            FadeGlobalVolume(BaseAParam, 1f, initialFadeDuration).Forget();
            float masterVolume = PlayerPrefs.GetFloat("Audio_Master", 0.5f);
            FadeGlobalVolume(MasterParam, masterVolume, initialFadeDuration).Forget();
        }

        /// <summary>
        /// Toggles the extra sound parameter.
        /// </summary>
        public void ToggleExtraSound(bool enable)
        {
            SetVolume(ActionParam, enable ? 1f : 0f);
        }

        /// <summary>
        /// Immediately swaps alternating sounds by setting global parameters.
        /// </summary>
        public void SwapAlternatingSounds()
        {
            _useAltA = !_useAltA;
            SetVolume(BaseAParam, _useAltA ? 1f : 0f);
            SetVolume(BaseBParam, _useAltA ? 0f : 1f);
        }

        /// <summary>
        /// Crossfades the alternating sounds over the given duration.
        /// </summary>
        public void CrossfadeAlternatingSounds(float duration)
        {
            string fadingOut = _useAltA ? BaseAParam : BaseBParam;
            string fadingIn  = _useAltA ? BaseBParam : BaseAParam;
            FadeGlobalVolume(fadingOut, 0f, duration).Forget();
            FadeGlobalVolume(fadingIn, 1f, duration).Forget();
            _useAltA = !_useAltA;
        }

        /// <summary>
        /// Plays a UI sound using an FMOD event.
        /// Pass in the FMOD event path (e.g., "event:/UI/Click").
        /// </summary>
        public void PlayUISound(string uiEventPath)
        {
            RuntimeManager.PlayOneShot(uiEventPath);
        }

        /// <summary>
        /// Fades a given FMOD global parameter from its current value to a target value over a duration.
        /// </summary>
        private async UniTaskVoid FadeGlobalVolume(string paramName, float targetVolume, float duration)
        {
            // Get the current value of the global parameter
            float currentVolume;
            RuntimeManager.StudioSystem.getParameterByName(paramName, out currentVolume);
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float newVolume = Mathf.Lerp(currentVolume, targetVolume, time / duration);
                RuntimeManager.StudioSystem.setParameterByName(paramName, newVolume);
                await UniTask.Yield();
            }
            RuntimeManager.StudioSystem.setParameterByName(paramName, targetVolume);
        }

        /// <summary>
        /// Sets a FMOD global parameter to the specified volume (linear, 0–1).
        /// </summary>
        private void SetVolume(string paramName, float volume)
        {
            RuntimeManager.StudioSystem.setParameterByName(paramName, volume);
        }
    }
}
