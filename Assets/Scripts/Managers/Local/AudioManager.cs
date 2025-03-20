using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.Rendering;

namespace Managers.Local
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance { get; private set; }
        
        [Header("Volumes")] 
        [UnityEngine.Range(0, 1)]
        public float masterVolume = 1;
        [UnityEngine.Range(0, 1)] 
        public float sfxVolume = 0;
        [UnityEngine.Range(0, 1)] 
        public float musicVolume = 0;
        [UnityEngine.Range(0, 1)] 
        public float ambienceVolume = 0;

        private List<EventInstance> eventInstances;
        private List<StudioEventEmitter> eventEmitters;
        
        private EventInstance ambienceInstance;
        private EventInstance musicEventInstance; //Make sure that the music loops through the entirety of the game
        
        private Bus Master;
        private Bus SoundEffect;
        private Bus Music;
        private Bus Ambience;

        

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("More than one music manager instance detected.");
            }
            instance = this;
            
            Master = RuntimeManager.GetBus("Bus:/");
            SoundEffect = RuntimeManager.GetBus("Bus:/SFXs");
            Music = RuntimeManager.GetBus("Bus:/Music");
            Ambience = RuntimeManager.GetBus("Bus:/Ambience");

            eventInstances = new List<EventInstance>();
            eventEmitters = new List<StudioEventEmitter>();
        }

        private void Start()
        {
            InitializeMusic(FMODEvents.instance.musicReference);
        }

        public void SetMusicVolume(float num)
        {
            musicVolume = num;
        }

        public void PlayOneShot(EventReference sound, Vector3 worldPos)
        {
            RuntimeManager.PlayOneShot(sound, worldPos);
        }

        public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterObject)
        {
            StudioEventEmitter emitter = emitterObject.GetComponent<StudioEventEmitter>();
            emitter.EventReference = eventReference;
            eventEmitters.Add(emitter);
            return emitter;
        }
        
        private void InitializeMusic(EventReference musicEventReference)
        {
            musicEventInstance = CreateInstance(musicEventReference);
            musicEventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(new Vector3(0,0,0)));
            musicEventInstance.start();
        }

        EventInstance CreateInstance(EventReference eventReference)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
            eventInstances.Add(eventInstance);
            return eventInstance;
        }

        void CleanUp()
        {
            foreach (EventInstance instances in eventInstances)
            {
                instances.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                instances.release();
            }

            foreach (var emitters in eventEmitters)
            {
                emitters.Stop();
            }
        }

        private void OnDestroy()
        {
            CleanUp();
        }
    }
}
