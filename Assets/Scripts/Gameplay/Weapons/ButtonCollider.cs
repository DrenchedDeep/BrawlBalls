using System;
using Gameplay;
using UnityEngine;
using UnityEngine.UI;

public class ButtonCollider : MonoBehaviour
{
    [SerializeField] private TheButtonWeapon wpn;
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<BallPlayer>())
        {
            wpn.Explode_ServerRpc();
        }
    }
}
