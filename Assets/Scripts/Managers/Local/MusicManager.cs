using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace Managers.Local
{
    public class MusicManager : MonoBehaviour
    {
        private const string ActionPath = "Action";
        private const string BaseAPath = "BaseA";
        private const string BaseBPath = "BaseB";
        
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioSource uiSource;
        

        private bool _useAltA = true; // Track which alternating sound is active

        public static MusicManager Instance { get; private set; }
        
        //This has to be start.
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
                                    
            // Ensure initial volumes are set correctly using log scale
            SetVolume(ActionPath, 0f);   // Mute initially
            SetVolume(BaseAPath, 0f);   // Mute initially
            SetVolume("Master", 0f);   // Mute initially
            SetVolume(BaseBPath, 0f);    // AltB muted
            const float initialFadeDuration = 3;
            
            FadeMixerGroup(BaseAPath, 1f, initialFadeDuration);
            FadeMixerGroup("Master", PlayerPrefs.GetFloat("Audio_Master", .5f), initialFadeDuration);
        }

        public void ToggleExtraSound(bool enable)
        {
            SetVolume(ActionPath, enable ? 1f : 0f);
        }

        public void SwapAlternatingSounds()
        {
            _useAltA = !_useAltA;
            SetVolume(BaseAPath, _useAltA ? 1f : 0f);
            SetVolume(BaseBPath, _useAltA ? 0f : 1f);
        }

        public void CrossfadeAlternatingSounds(float duration)
        {
            FadeMixerGroup(_useAltA ? BaseAPath : BaseBPath, 0f, duration);
            FadeMixerGroup(_useAltA ? BaseBPath : BaseAPath, 1f, duration);
            _useAltA = !_useAltA;
        }

        public void PlayUISound(AudioClip clip)
        {
            uiSource.PlayOneShot(clip);   
        }

        private async void FadeMixerGroup(string param, float targetLinearVolume, float duration)
        {
            audioMixer.GetFloat(param, out float currentDb);
            float startLinear = DbToLinear(currentDb);
            float time = 0;

            while (time < duration)
            {
                time += Time.deltaTime;
                float newLinearVolume = Mathf.Lerp(startLinear, targetLinearVolume, time / duration);
                SetVolume(param, newLinearVolume);
                await UniTask.Yield();
            }
            SetVolume(param, targetLinearVolume);
        }

        private void SetVolume(string param, float linearVolume)
        {
            float dB = LinearToDb(linearVolume);
            audioMixer.SetFloat(param, dB);
        }

        private float LinearToDb(float linear)
        {
            linear = Mathf.Clamp(linear, 0.0001f, 1f); // Prevent log(0)
            return 20f * Mathf.Log10(linear);
        }

        private float DbToLinear(float dB)
        {
            return Mathf.Pow(10f, dB / 20f);
        }
    }
}
