using System.Collections;
using Gameplay;
using Unity.Netcode;
using UnityEngine;

public class Spike : Ability
{

    public override bool CanUseAbility(Ball owner, Weapon weapon)
    {
        return owner.Speed > 3;
    }

    protected override void UseAbility(Ball owner, Weapon weapon)
    {
        Debug.Log("Attacked!");
        //Un parent self
        weapon.Disconnect(owner.Speed);
        
    }
    
    public static IEnumerator Move(Weapon weapon, float speed)
    {
        Transform ownerTrans = weapon.transform;
        float duration = 5;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            ownerTrans.position += speed * Time.deltaTime * ownerTrans.forward;
            if (Physics.Raycast(ownerTrans.position, ownerTrans.forward, out RaycastHit hit, weapon.Range.y * 10, GameManager.GroundLayers)) // 1==Default
            {
                ownerTrans.position = hit.point - ownerTrans.forward * (weapon.Range.y * 10f);
                weapon.enabled = false;
                yield break;
            }
            yield return null;
        }
        weapon.NetworkObject.Despawn();
    }
}
