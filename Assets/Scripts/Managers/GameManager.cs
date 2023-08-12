using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    //Includes both ground (default) and bouncey layers.
    public static int GroundLayers { get; private set; }
    public static int ImmortalLayer { get; private set; }
    public static int PlayerLayers { get; private set; }
    public static bool IsOnline { get; set; } //Netamanager. ..?
    public static bool GameStarted { get; set; }
    
    


    [SerializeField] private Ball[] ballIds;
    [SerializeField] private Weapon[] weaponIds;
    [SerializeField] private AbilityStats[] abilityIds;

    public static Dictionary<string, Ball> Balls = new();
    public static Dictionary<string, Weapon> Weapons = new();
    public static Dictionary<string, AbilityStats> Abilities =  new();

    [SerializeField] private NetworkObject hull;
    public static NetworkObject Hull { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Hull = hull;
        GroundLayers = (1 << LayerMask.NameToLayer("Default"));
        PlayerLayers = (1 << LayerMask.NameToLayer("Enemy")) + (1 << LayerMask.NameToLayer("Ball"));
        ImmortalLayer = (LayerMask.NameToLayer("Immortal"));
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
