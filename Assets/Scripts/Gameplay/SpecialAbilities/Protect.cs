using System.Collections;
using Gameplay;
using UnityEngine;

public class Protect : Ability
{
    
    public override bool CanUseAbility(Ball owner, Weapon weapon)
    {
        return true;
    }

    protected override void UseAbility(Ball owner, Weapon weapon)
    {
        HostSetImmortal(owner);
    }

    private void HostSetImmortal(Ball owner)
    {
        owner.StartCoroutine(ImmortalityTimer(owner));
    }

    private IEnumerator ImmortalityTimer(Ball owner)
    {
        Debug.Log("Im_Start");
        int id = owner.AddMaterial(ParticleManager.ProtectMat);

        GameObject t = owner.transform.GetChild(0).gameObject;
        int prvL = t.layer;
        t.layer = GameManager.ImmortalLayer;
        
        yield return new WaitForSeconds(3);
        Debug.Log("Im_End");
        
        t.layer = prvL;
        owner.RemoveMaterial(id);
    }

}
