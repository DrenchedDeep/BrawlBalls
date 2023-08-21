using Gameplay;
using Gameplay.Object_Scripts;
using UnityEngine;

public class Caltrop : Ability
{
    public override bool CanUseAbility(Ball owner, Weapon weapon)
    {
        //Not possible to fail...
        return true;
    }

    protected override void UseAbility(Ball owner, Weapon weapon)
    {
        Level.Instance.SpawnObjectGlobally_ServerRpc("Caltrop", owner.transform.GetChild(0).position);
        

    }
}
