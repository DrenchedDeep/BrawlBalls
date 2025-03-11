using System.Collections.Generic;
using Gameplay;
using Stats;
using Unity.Netcode;
using UnityEngine;
using BallPlayer = Gameplay.BallPlayer;

namespace Managers.Local
{
    
    public class ResourceManager : MonoBehaviour
    {
        //public static bool IsOnline { get; set; } //Netamanager. ..?
        [SerializeField] private BallPlayer hull;

        
        [Header("Ball things... Addressablse?")]
        [SerializeField] private Ball[] ballIds;
        [SerializeField] private Weapon[] weaponIds;
        [SerializeField] private AbilityStats[] abilityIds;
        [SerializeField] private NetworkObject[] summonableObjects;


        public static readonly Dictionary<string, Ball> Balls = new();
        public static readonly Dictionary<string, Weapon> Weapons = new();
        public static readonly Dictionary<string, AbilityStats> Abilities = new();
        public static readonly Dictionary<string, NetworkObject> SummonableObjects = new();

        
        public static BallPlayer Hull { get; set; }

        // Start is called before the first frame update
        void Awake()
        {
            
            Debug.LogError("Can this be converted to Addressables?", gameObject);
            
            Hull = hull;
            
            Application.targetFrameRate = -1; // native default... (BIND IN SETTINGS LATER)
            
            Balls.Clear();
            Weapons.Clear();
            Abilities.Clear();
            SummonableObjects.Clear();
            
            foreach (Ball b in ballIds)
            {
                Balls.Add(b.name, b);
            }
        
            foreach (Weapon b in weaponIds)
            {
                Weapons.Add(b.name, b);
            }
            
            foreach (AbilityStats b in abilityIds)
            {
                Abilities.Add(b.name, b);
            }
            
            foreach (NetworkObject b in summonableObjects)
            {
                SummonableObjects.Add(b.name, b);
            }

        }
        
        public static BallPlayer CreateBallDisabled(string ball, string weapon, Vector3 location, Quaternion rotation)
        {
                            
            //Create the Ball Controller
            BallPlayer createdBallPlayer = Instantiate(Hull, location, rotation);
            Transform cachedTransform = createdBallPlayer.transform;
            
            Ball createdBall = Instantiate(Balls[ball], cachedTransform);
            Weapon createdWeapon = Instantiate(Weapons[weapon], cachedTransform);

            createdBallPlayer.enabled = false;
            createdBall.enabled = false;
            createdWeapon.enabled = false;
            
            return createdBallPlayer;
        }
    }
}
