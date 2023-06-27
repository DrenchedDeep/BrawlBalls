using System;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    
    [SerializeField] protected WeaponStats stats;

    //[Header("Weapon Object")]
    [field: SerializeField] public bool HasAbility { get; private set; }
    [SerializeField] protected LayerMask hitLayers;

    protected readonly RaycastHit[] hits = new RaycastHit[10];
    
    public float Mass => stats.Mass;
    protected Rigidbody root;
    private void Start()
    {
        root = transform.parent.GetChild(0).GetComponent<Rigidbody>();
    }


    //Default update, always check forward, and if hitting enemy then do thing...
    protected virtual void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        Vector3 velocity = root.velocity;
        Vector3 dir = Vector3.Lerp(Vector3.up, velocity.normalized, velocity.sqrMagnitude);
        
        transform.position = root.position + dir * stats.BaseDist;
        transform.forward = dir;
    }
    
    

    protected void CastForward()
    {
        int iterations = Physics.BoxCastNonAlloc(transform.position + transform.forward * stats.Range.z, stats.Range,
            transform.forward, hits, Quaternion.identity, stats.Range.z, hitLayers);

        while (--iterations >= 0)
        {
            HitObject(hits[iterations]);
        }
    }

    protected virtual bool HitObject(RaycastHit hit)
    {
        Transform n = hit.transform.parent;
        if (n && n.TryGetComponent(out Ball b) && n != transform.parent)
        {
            //FIX this doesn't consider speed...
            float dmg = Damage();
            print(b + "took damage --> " + dmg);
            b.TakeDamage(dmg, dmg*0.1f *  transform.forward, Player.LocalPlayer);
            return true;
        }

        return false;
    }

    public virtual float Damage()
    {
        return stats.Damage;
    }

    public virtual void UseAbility()
    {
        
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, Vector3.up * stats.BaseDist);
        ExtDebug.DrawBox(transform.position + transform.forward *stats.Range.z, stats.Range, transform.rotation, Color.red);
    }
    #endif
    
}
