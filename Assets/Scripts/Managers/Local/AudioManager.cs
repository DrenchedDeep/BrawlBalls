using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

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
        
        private EventInstance ambienceEventInstance;
        private EventInstance musicEventInstance; //Make sure that the music loops through the entirety of the game
        

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("More than one music manager instance detected.");
            }
            instance = this;

            eventInstances = new List<EventInstance>();
            eventEmitters = new List<StudioEventEmitter>();
        }

        private void Start()
        {
            InitializeMusic(FMODEvents.instance.musicReference);
            InitializeAmbience(FMODEvents.instance.ambienceReference);
        }

        public void SetMusicVolume(float num)
        {
            musicVolume = num;
        }

        public void PlayOneShot(EventReference sound, Vector3 worldPos)
        {
            RuntimeManager.PlayOneShot(sound, worldPos);
        }

        void InitializeAmbience(EventReference soundReference)
        {
            ambienceEventInstance = CreateInstance(soundReference);
            ambienceEventInstance.start();
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
            musicEventInstance.start();
        }

        public void TriggerAction(int volume)
        {
            RuntimeManager.StudioSystem.setParameterByName("Action", volume);
        }

        public void TriggerInSelection(int volume)
        {
            RuntimeManager.StudioSystem.setParameterByName("InSelection", volume);
        }

        public void TriggerGameState(int volume)
        {
            musicEventInstance.setParameterByName("GameState", volume);
        }

        public void TriggerDeath(int volume)
        {
            RuntimeManager.StudioSystem.setParameterByName("Death", volume);
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
