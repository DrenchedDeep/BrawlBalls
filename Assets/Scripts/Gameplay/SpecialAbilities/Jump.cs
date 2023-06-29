using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : Ability
{
    protected override bool CanUseAbility(Ball owner, Weapon weapon, out string failText)
    {
        //Only allow jump if grounded.
        failText = "must be on ground!";
        return Physics.Raycast(owner.transform.GetChild(0).position, Vector3.down, 1, GameManager.GroundLayers); 
    }

    protected override void UseAbility(Ball owner, Weapon weapon)
    {
        owner.AddVelocity(50 * Vector3.up);
        ParticleManager.InvokeParticle("Jump", owner.transform.GetChild(0).position);
    }
}
