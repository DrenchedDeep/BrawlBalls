using Gameplay;
using UnityEngine;

public class Portal : Ability
{
    public override bool CanUseAbility(Ball owner, Weapon weapon)
    {
        return Physics.Raycast(owner.transform.GetChild(0).position, Vector3.down, 1, GameManager.GroundLayers); 
    }

    protected override void UseAbility(Ball owner, Weapon weapon)
    {
        Object.Instantiate(ParticleManager.SummonObjects["Portal"], owner.transform.GetChild(0).position, Quaternion.Euler(-90,0,0));
    }
}
