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
        
        int refMat = ParticleManager.ProtectMat.GetHashCode();
        owner.ApplyEffectServerRpc(1);

        yield return new WaitForSeconds(3);
        Debug.Log("Im_End");
        
        owner.RemoveEffectServerRpc(refMat);
    }

}
