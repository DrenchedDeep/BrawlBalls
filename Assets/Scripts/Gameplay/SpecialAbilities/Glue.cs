using Gameplay;
using Gameplay.Object_Scripts;
using UnityEngine;

public class Glue : Ability
{

    public override bool CanUseAbility(Ball owner, Weapon weapon)
    {
        return Physics.Raycast(owner.transform.GetChild(0).position, Vector3.down, 1, GameManager.GroundLayers);
    }

    protected override void UseAbility(Ball owner, Weapon weapon)
    {
        Level.Instance.SpawnObjectGlobally_ServerRpc("Glue", owner.transform.GetChild(0).position);
    }
}
