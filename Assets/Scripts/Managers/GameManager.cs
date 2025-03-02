using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class GameManager : MonoBehaviour
{

    //Includes both ground (default) and bouncey layers.
    public static int GroundLayers { get; private set; }
    public static int ImmortalLayer { get; private set; }
    public static int PlayerLayers { get; private set; }
    public static int LocalLayer { get; private set; }
    public static int EnemyLayer { get; private set; }
    public static bool IsOnline { get; set; } //Netamanager. ..?
    public static bool GameStarted { get; set; }
    
    


    [SerializeField] private Ball[] ballIds;
    [SerializeField] private Weapon[] weaponIds;
    [SerializeField] private AbilityStats[] abilityIds;

    public static Dictionary<string, Ball> Balls;
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
        GroundLayers = 1<<LayerMask.NameToLayer("Default"); // 1<< for raycast
        LocalLayer = LayerMask.NameToLayer("Ball");
        EnemyLayer = LayerMask.NameToLayer("Enemy");
        PlayerLayers = (1<<LocalLayer) + (1<<EnemyLayer);
        ImmortalLayer = LayerMask.NameToLayer("Immortal");
        Application.targetFrameRate = -1; // native default... (BIND IN SETTINGS LATER)
        Balls = new();
        foreach (Ball b in ballIds)
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
