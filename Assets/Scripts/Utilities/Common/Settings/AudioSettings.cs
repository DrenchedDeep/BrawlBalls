using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Utilities.Common.Settings
{
    public class AudioSettings : MonoBehaviour, ISettingsMenu
    {
        [SerializeField] private AudioPairing[] audioPairings;

        [Serializable]
        private struct AudioPairing
        {
            public Slider slider;
            public AudioMixer targetGroup;
            public string targetName;
        }
        
        private void Awake()
        {
            for (int i = 0; i < audioPairings.Length; i++)
            {
                int trueIndex = i;
                AudioPairing pair = audioPairings[i];
                pair.slider.onValueChanged.AddListener(_ => ChangedAudioValue(audioPairings[trueIndex]));
            }
        }


        private void ChangedAudioValue(AudioPairing pair)
        {
            pair.targetGroup.SetFloat(pair.targetName, Mathf.Log10(pair.slider.value  ) * 20);
        }


        public void Save()
        {
            foreach (AudioPairing pair in audioPairings)
            {
                PlayerPrefs.SetFloat("Audio_"+pair.targetName,pair.slider.value);
            }
        }

        public void Load()
        {
            foreach (AudioPairing pair in audioPairings)
            {
                float value = PlayerPrefs.GetFloat("Audio_"+pair.targetName, 0.5f);
                pair.slider.value = value;
            }
        }
    }
}
