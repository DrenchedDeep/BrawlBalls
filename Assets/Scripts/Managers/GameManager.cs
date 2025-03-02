using System.Collections.Generic;
using Gameplay.Balls;
using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static bool IsOnline { get; set; } //Netamanager. ..?
        public static bool GameStarted { get; set; }

        [SerializeField] private NetworkBall[] ballIds;
        [SerializeField] private Weapon[] weaponIds;
        [SerializeField] private AbilityStats[] abilityIds;

        public static Dictionary<string, NetworkBall> Balls;
        public static Dictionary<string, Weapon> Weapons;
        public static Dictionary<string, AbilityStats> Abilities;
    
    
        [SerializeField] private NetworkObject hull;
    


        public static NetworkObject Hull { get; private set; }
    
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInit()
        {
            Balls = new();
            Weapons = new();
            Abilities = new();
        }

        // Start is called before the first frame update
        void Start()
        {
            Hull = hull;
            Application.targetFrameRate = -1; // native default... (BIND IN SETTINGS LATER)
            Balls = new();
            foreach (NetworkBall b in ballIds)
            {
                Balls.Add(b.name, b);
            }
        
            Weapons = new();
            foreach (Weapon b in weaponIds)
            {
                Weapons.Add(b.name, b);
            }
        
            Abilities = new();
            foreach (AbilityStats b in abilityIds)
            {
                Abilities.Add(b.name, b);
            }
        }


   

    }
}
