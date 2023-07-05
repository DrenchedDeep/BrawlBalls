using System.Collections;
using System.Collections.Generic;
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
        GameObject go = Object.Instantiate(ParticleManager.SummonObjects["Glue"], owner.transform.GetChild(0).position, Quaternion.identity);
        go.transform.GetChild(0).GetComponent<PlaceableObject>().Init(owner);
    }
}
