using System.Collections.Generic;
using Core.ActionMaps;
using Gameplay.Balls;
using Gameplay.Weapons;
using Stats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using BallPlayer = Gameplay.BallPlayer;

namespace Managers.Local
{
    
    [DefaultExecutionOrder(-1000)]
    public class ResourceManager : MonoBehaviour
    {
        //public static bool IsOnline { get; set; } //Netamanager. ..?

        [Header("Ball things... Addressablse?")] 
        [SerializeField] private BallPlayer hull;
        [SerializeField] private Ball[] ballIds;
        [SerializeField] private BaseWeapon[] weaponIds;
        [SerializeField] private AbilityStats[] abilityIds;
        [SerializeField] private NetworkObject[] summonableObjects;

        [SerializeField] private Sprite[] rarityIcons;
        [SerializeField] private Color[] rarityColors;
#if !UNITY_ANDROID && !UNITY_IOS
        [SerializeField] private InputSpriteActionMap[] actionMaps;
#endif

            
        public static readonly Dictionary<string, Ball> Balls = new();
        public static readonly Dictionary<string, BaseWeapon> Weapons = new();
        public static readonly Dictionary<string, AbilityStats> Abilities = new();
        public static readonly Dictionary<string, NetworkObject> SummonableObjects = new();
        
        public static ResourceManager Instance { get; private set; }
        public BallPlayer Hull => hull;


        // Start is called before the first frame update
        void OnEnable()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.LogWarning("Can this be converted to Addressables?", gameObject);
            
            Application.targetFrameRate = -1; // native default... (BIND IN SETTINGS LATER)
            
            Balls.Clear();
            Weapons.Clear();
            Abilities.Clear();
            SummonableObjects.Clear();
            
            foreach (Ball b in ballIds)
            {
                Balls.Add(b.name, b);
            }
        
            foreach (BaseWeapon b in weaponIds)
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
#if !UNITY_ANDROID && !UNITY_IOS
        public InputSpriteActionMap GetActionMap(ReadOnlyArray<InputDevice> inputDevices)
        {
            InputSpriteActionMap map = actionMaps[0];
            for (int i = 1; i < actionMaps.Length; ++i)
            {
                Debug.Log("Trying to find action map for: " + inputDevices[i].displayName);
                foreach (var t in inputDevices)
                {
                    if (t.displayName.Equals(actionMaps[i].DisplayName)) return map;
                }
            }
            return map;
        }
#endif
        public static BallPlayer CreateBallDisabled(string ball, string weapon, Transform root,out Ball b, out BaseWeapon w)
        {
                            
            //Create the Ball Controller
            BallPlayer createdBallPlayer = Instantiate(Instance.hull, root);
            Transform cachedTransform = createdBallPlayer.transform;
            
            b = Instantiate(Balls[ball], cachedTransform);
            w = Instantiate(Weapons[weapon], cachedTransform);

            createdBallPlayer.enabled = false;
            b.enabled = false;
            w.enabled = false;
            
            return createdBallPlayer;
        }

        public void GetRarityInformation(ERarity statsRarity, out Color color, out Sprite image)
        {
            int id = (int)statsRarity;
            color = rarityColors[id];
            image = rarityIcons[id];
        }
    }
}
