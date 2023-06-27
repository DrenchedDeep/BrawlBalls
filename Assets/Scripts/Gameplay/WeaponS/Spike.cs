using UnityEngine;
using UnityEngine.VFX;

public class Spike : Weapon
{
    private float speed;
    private bool stuck;
    private bool moving;
    protected override void Update()
    {
        if (!moving)
            base.Update();
        else
            transform.position += speed * Time.deltaTime * transform.forward;

        CastForward();

    }

    public override float Damage()
    {
        return stats.Damage * stats.Mass * (moving ? speed:root.velocity.magnitude);
    }

    protected override bool HitObject(RaycastHit hit)
    {
        if (base.HitObject(hit)) return true;
        if (hit.transform.gameObject.layer == 0)
        {
            GetComponent<BoxCollider>().enabled = true;
            transform.position = hit.point - transform.forward * (stats.Range.z*1.8f);
            Destroy(this);
            return true;
        }
        return false;
    }


    public override void UseAbility()
    {
        //Un parent self
        transform.parent = null;
        moving = true;
        
        
        print(hitLayers.value);
        hitLayers += 1;
        print(hitLayers.value);
        Player.LocalPlayer.SetWeaponAbilityState(false);

        speed = root.velocity.magnitude * 5;
        root.velocity = Vector3.zero;
        //Detach from ball

    }
}
