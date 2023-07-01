using Gameplay;
using Gameplay.Object_Scripts;
using UnityEngine;

public class Caltrop : Ability
{
    protected override bool CanUseAbility(Ball owner, Weapon weapon, out string failText)
    {
        //Not possible to fail...
        failText = "";
        return true;
    }

    protected override void UseAbility(Ball owner, Weapon weapon)
    {
        GameObject go = Object.Instantiate(ParticleManager.SummonObjects["Caltrop"], owner.transform.GetChild(0).position, Quaternion.identity);
        Rigidbody rb = go.GetComponent<Rigidbody>();
        rb.AddForce(Vector3.up * 10, ForceMode.Impulse);
        rb.AddTorque(weapon.transform.forward * -10, ForceMode.Impulse);
        go.transform.GetChild(0).GetComponent<PlaceableObject>().Init(owner);

    }
}
