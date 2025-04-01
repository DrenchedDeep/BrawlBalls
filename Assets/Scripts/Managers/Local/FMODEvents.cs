using System.Collections.Generic;
using UnityEngine;
using EventReference = FMODUnity.EventReference;

namespace Managers.Local
{
    public class FMODEvents : MonoBehaviour
    {
        private float pitchModu = 0f;
        private float timer = 15;

        [field: Header("BGM")]
        [field: SerializeField]
        public EventReference musicReference { get; private set; }
        
        [field: Header("Ambience")]
        [field: SerializeField]
        public EventReference ambienceReference { get; private set; }
        
        [field: Header("Ball Explosions")]
        [field: SerializeField]
        public EventReference confettiExplosion { get; private set; }
        
        [field: SerializeField]
        public EventReference soccerExplosion { get; private set; }
        
        [field: SerializeField]
        public EventReference canonExplosion { get; private set; }
        
        [field: SerializeField]
        public EventReference paintballExplosion { get; private set; }
        
        [field: Header("Abilities")]
        [field: SerializeField]
        public EventReference RocketExplosion { get; private set; }

        [field: Header("Spawn Ball")]
        [field: SerializeField]
        public List<EventReference> spawnBall { get; private set; }
        
        [field: Header("Customization Scroll")]
        [field: SerializeField]
        public EventReference scroll { get; private set; }
        
        [field: Header("Customization Click")]
        [field: SerializeField]
        public EventReference click { get; private set; }
        
        [field: Header("Customization Click")]
        [field: SerializeField]
        public List<EventReference> Beep { get; private set; }
        
        [field: Header("Jump Sound")]
        [field: SerializeField]
        public EventReference Jump { get; private set; }

        public static FMODEvents instance { get; private set; }

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("More than one FMODEvents found.");
            }

            instance = this;
        }

        public void PlayEvent(string command)
        {
            if (command == "scroll")
            {
                timer -= 1;
                if (timer < 15f)
                {
                    pitchModu += 0.1f;
                }
                
                AudioManager.instance.PlayOneShot(scroll, transform.position);
            }

            if (command == "click")
            {
                timer = 15;
                pitchModu = 0f;
                AudioManager.instance.PlayOneShot(click, transform.position);
            }
        }
    }
}