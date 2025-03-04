using System;
using Managers.Local;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers.Network
{
    public class EffectHandler : NetworkBehaviour
    {
        public static EffectHandler Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            Debug.LogWarning("Implement the EffectHandler, move code out of Ball.");
        }

  
        
        
    }
}