using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Utilities.Common.Settings
{
    public class AudioSettings : MonoBehaviour, ISettingsMenu
    {
        [SerializeField] private AudioPairing[] audioPairings;

        [Serializable]
        private class AudioPairing
        {
            public Slider slider;
            private Bus _bus;
            [SerializeField] private string busPath;

            public Bus GetBus()
            {
                if (!_bus.hasHandle()) _bus = RuntimeManager.GetBus(busPath);
                return _bus;
            }

            public void SetVolume(float volume)
            {
                GetBus().setVolume(volume); // Convert back to linear
            }

            public void Save()
            {
                PlayerPrefs.SetFloat("Audio_" + busPath, slider.value);
            }

            public void Load()
            {
                float value = PlayerPrefs.GetFloat("Audio_" + busPath, 0.5f);
                slider.value = value;
            }

        }
        
        private void Awake()
        {
            foreach (var t in audioPairings)
            {
                t.slider.onValueChanged.AddListener(t.SetVolume);
            }
        }

        public void Save()
        {
            foreach (AudioPairing pair in audioPairings)
            {
                pair.Save();
            }
        }

        public void Load()
        {
            foreach (AudioPairing pair in audioPairings)
            {
                pair.Load();
            }
        }
    }
}
