using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    //Includes both ground (default) and bouncey layers.
    public static int GroundLayers { get; private set; }
    

    // Start is called before the first frame update
    void Awake()
    {
        GroundLayers = (1 << LayerMask.NameToLayer("Default")) + (1 << LayerMask.NameToLayer("Bouncy"));
    }
}
