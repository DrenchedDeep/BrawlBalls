using System.Collections;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;

public class Jump : Ability
{
    public override bool CanUseAbility(Ball owner, Weapon weapon)
    {
        //Only allow jump if grounded.
        return Physics.Raycast(owner.transform.GetChild(1).position, Vector3.down, 1, GameManager.GroundLayers); 
    }

    protected override void UseAbility(Ball owner, Weapon weapon)
    {
        owner.ChangeVelocity(50 * Vector3.up);
        Level.Instance.PlayParticleGlobally_ServerRpc("Jump", owner.transform.GetChild(1).position);
    }
}
