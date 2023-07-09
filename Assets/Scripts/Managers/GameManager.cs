using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    //Includes both ground (default) and bouncey layers.
    public static int GroundLayers { get; private set; }
    public static int ImmortalLayer { get; private set; }
    public static int PlayerLayers { get; private set; }


    // Start is called before the first frame update
    void Awake()
    {
        GroundLayers = (1 << LayerMask.NameToLayer("Default"));
        PlayerLayers = (1 << LayerMask.NameToLayer("Enemy")) + (1 << LayerMask.NameToLayer("Ball"));
        ImmortalLayer = (LayerMask.NameToLayer("Immortal"));
        Application.targetFrameRate = -1; // native default... (BIND IN SETTINGS LATER)
    }
}
