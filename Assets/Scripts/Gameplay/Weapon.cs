using System;
using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    
    [SerializeField] private WeaponStats stats;

    //[Header("Weapon Object")]
    public bool HasAbility => stats.Ability.MyAbility != null;
    [SerializeField] private LayerMask hitLayers;

    private readonly RaycastHit[] hits = new RaycastHit[10];
    
    public float Mass => stats.Mass;
    public Vector3 Range => stats.Range;
    private Ball owner;
    private Rigidbody root;
    private bool isConnected = true;

    public AbilityStats GetAbility => stats.Ability;
    
    // Ability stuffs
    
    private void Start()
    {
        owner = transform.parent.GetComponent<Ball>();
        root = owner.transform.GetChild(0).GetComponent<Rigidbody>();
    }


    //Default update, always check forward, and if hitting enemy then do thing...
    protected virtual void Update()
    {
        if(isConnected)
            Rotate();
        CastForward();
    }

    private void Rotate()
    {
        Vector3 velocity = root.velocity;
        Vector3 dir = Vector3.Lerp(Vector3.up, velocity.normalized, velocity.sqrMagnitude);
        
        transform.position = root.position + dir * stats.BaseDist;
        transform.forward = dir;
    }
    
    

    private void CastForward()
    {
        Vector3 forward = transform.forward;
        int iterations = Physics.BoxCastNonAlloc(transform.position + forward * stats.Range.z, stats.Range,
            forward, hits, Quaternion.identity, stats.Range.z, hitLayers);

        while (--iterations >= 0)
        {
            HitObject(hits[iterations]);
        }
    }

    private void HitObject(RaycastHit hit)
    {
        Transform n = hit.transform.parent;
        if (n && n.TryGetComponent(out Ball b) && n != transform.parent)
        {
            //FIX this doesn't consider speed...
            float dmg = Damage();
            print(b + "took damage --> " + dmg);
            b.TakeDamage(dmg, dmg*0.1f *  transform.forward, Player.LocalPlayer);
        }
    }

    public float Damage()
    {
        return stats.ForceBasedDamage ? stats.Damage * root.mass * owner.Speed:stats.Damage;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, Vector3.up * stats.BaseDist);
        ExtDebug.DrawBox(transform.position + transform.forward *stats.Range.z, stats.Range, transform.rotation, Color.red);
    }
    #endif

    public void Disconnect()
    {
        transform.parent = null;
        isConnected = false;
    }
}
