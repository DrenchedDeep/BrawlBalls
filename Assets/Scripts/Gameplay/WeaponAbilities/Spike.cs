using System.Collections;
using System.Net;
using UnityEngine;

public class Spike : Ability
{
    protected override void UseAbility()
    {
        Debug.Log("Attacked!");
        //Un parent self
        MyWeapon.Disconnect();
       
       
        
        Player.LocalPlayer.SetWeaponAbilityState(false);

        MyOwner.StartCoroutine(ConnectionTime());
        MyOwner.StartCoroutine(Move(MyOwner.Speed * 5));
    }
    
    private IEnumerator Move(float speed)
    {
        Transform ownerTrans = MyWeapon.transform;
        if (speed == 0)
        {
            Object.Destroy(MyWeapon);
            ownerTrans.GetComponent<BoxCollider>().enabled = true;
        }

        while (true)
        {
            ownerTrans.position += speed * Time.deltaTime * ownerTrans.forward;
            if (Physics.Raycast(ownerTrans.position, ownerTrans.forward, out RaycastHit hit, MyWeapon.Range.z * 2, 1)) // 1==Default
            {
                
                MyOwner.StopCoroutine(ConnectionTime());
                ownerTrans.position = hit.point - ownerTrans.forward * (MyWeapon.Range.z * 1.8f);
                Object.Destroy(MyWeapon);
                ownerTrans.GetComponent<BoxCollider>().enabled = true;
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator ConnectionTime()
    {
        yield return new WaitForSeconds(5);
        MyOwner.StopCoroutine(Move(5));
         if(MyWeapon) 
             Object.Destroy(MyWeapon.gameObject);
    }
}
