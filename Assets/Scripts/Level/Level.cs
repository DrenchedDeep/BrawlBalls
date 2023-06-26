using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private float bottomY;
    [SerializeField] private bool offMapKills;
    [field: SerializeField] public bool IsRandomSpawning { get; private set; }

    public static Level Instance { get; private set; }

    private void Awake()
    {
        if(Instance) Destroy(gameObject);
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //Every player is responsible for checking if they've fallen off the map? Or should it be the server... probably the server...
        if (Player.LocalPlayer.BallY < bottomY)
        {
            if (offMapKills)
            {
                //Player.LocalPlayer.TakeDamage(100000);
            }
            else
            {
                Player.LocalPlayer.Respawn(false);
            }
        }
    }
    
    #if UNITY_EDITOR
    [SerializeField] private bool display;
    private void OnDrawGizmos()
    {
        if (!display) return;
        Gizmos.color = Color.black;
        Gizmos.DrawCube(Vector3.up * bottomY, new Vector3(100000f,0.1f,100000f));
    }
#endif
}
