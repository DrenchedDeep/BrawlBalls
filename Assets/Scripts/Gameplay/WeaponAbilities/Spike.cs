using System.Collections;
using Gameplay;
using UnityEngine;

public class Spike : Ability
{

    public override bool CanUseAbility(Ball owner, Weapon weapon)
    {
        return owner.Speed > 3;
    }

    protected override void UseAbility(Ball owner, Weapon weapon)
    {
        Debug.Log("Attacked!");
        //Un parent self
        weapon.Disconnect();
        
    }
    
    public static IEnumerator Move(Ball owner, Weapon weapon, float speed)
    {
        Transform ownerTrans = weapon.transform;
        if (speed == 0)
        {
            Object.Destroy(weapon);
            ownerTrans.GetComponent<BoxCollider>().enabled = true;
        }

        while (ownerTrans)
        {
            ownerTrans.position += speed * Time.deltaTime * ownerTrans.forward;
            if (Physics.Raycast(ownerTrans.position, ownerTrans.forward, out RaycastHit hit, weapon.Range.y * 10, GameManager.GroundLayers)) // 1==Default
            {
                
                owner.StopCoroutine(ConnectionTime(weapon));
                ownerTrans.position = hit.point - ownerTrans.forward * (weapon.Range.y * 10f);
                weapon.gameObject.layer = 0;
                Object.Destroy(weapon);
                ownerTrans.GetComponent<BoxCollider>().enabled = true;
                yield break;
            }
            yield return null;
        }
    }

    public static IEnumerator ConnectionTime(Weapon weapon)
    {
        yield return new WaitForSeconds(5);
        if (weapon)
        {
            Object.Destroy(weapon.gameObject);
        }
    }
}
