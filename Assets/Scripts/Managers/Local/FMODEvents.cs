using System;
using Mono.Cecil;
using UnityEngine;
using FMODUnity;
using EventReference = FMODUnity.EventReference;

namespace Managers.Local
{
    public class FMODEvents : MonoBehaviour
    {
        [field: Header("Confetti Explosion")]
        [field: SerializeField]
        public EventReference confettiExplosion { get; private set; }

        [field: Header("Spawn Ball")]
        [field: SerializeField]
        public EventReference spawnBall { get; private set; }

        [field: Header("BGM")]
        [field: SerializeField]
        public EventReference musicReference { get; private set; }

        public static FMODEvents instance { get; private set; }

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("More than one FMODEvents found.");
            }

            instance = this;
        }
    }
}