using Gameplay;
using UnityEngine;

public class Portal : Ability
{
    public override bool CanUseAbility(Ball owner, Weapon weapon)
    {
        return Physics.CheckSphere(owner.transform.GetChild(0).position, 1, GameManager.GroundLayers); 
    }

    protected override void UseAbility(Ball owner, Weapon weapon)
    {
        Level.Instance.SpawnObjectGlobally_ServerRpc("Portal", owner.transform.GetChild(0).position);
    }
}
