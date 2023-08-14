using Gameplay.Object_Scripts;
using Unity.Netcode;
using UnityEngine;

public class CaltropObject : PlaceableObject
{

    protected override void OnHit(Ball hit)
    {
        //Again verify with upgrades and whatnot...
        //hit.TakeDamage(50, 15, owner.player);
        hit.TakeDamageClientRpc(hit.Speed*3, hit.Velocity * -5f + Vector3.up*20, NetworkManager.Singleton.LocalClientId);
        

    }
}
