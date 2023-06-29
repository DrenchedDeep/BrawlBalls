using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : Ability
{
    protected override void UseAbility()
    {
        MyOwner.AddVelocity(50 * Vector3.up);
        ParticleManager.InvokeParticle("Jump", MyOwner.transform.position);
    }
}
