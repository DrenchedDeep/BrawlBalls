using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private float bottomY;
    [SerializeField] private bool offMapKills;
    // Update is called once per frame
    void Update()
    {
        if (Player.LocalPlayer.BallY < bottomY)
        {
            if (offMapKills)
            {
                Player.LocalPlayer.TakeDamage(100000);
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
