using System;
using System.Collections;
using RotaryHeart.Lib.PhysicsExtension;
using UnityEngine;
using Physics = UnityEngine.Physics;

public class Weapon : MonoBehaviour
{
    
    [SerializeField] private WeaponStats stats;

    public readonly RaycastHit[] Hits = new RaycastHit[10];
    public int HitCount { get; private set; }

    public float Mass => stats.Mass;
    public Vector2 Range => stats.Range;
    private Ball owner;
    private Rigidbody root;
    private bool isConnected = true;

    public AbilityStats GetAbility => stats.Ability;

    [SerializeField] private bool isActive = true;
    public void ToggleActive()
    {
        isActive = !isActive;
    }


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
        if(isActive)
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
        Vector3 position = transform.position;
        Vector3 forward = transform.forward;
        
        Physics.Raycast(position, forward, out RaycastHit wallCheck,  stats.Range.x, GameManager.GroundLayers);
        float dist = wallCheck.distance;

        HitCount = Physics.SphereCastNonAlloc(position, stats.Range.y, forward, Hits, dist, stats.HitLayers);

        
        for (int i = 0; i < HitCount; ++i)
        {

            Transform n = Hits[i].transform.parent;

            if (n && n.TryGetComponent(out Ball b) && n != transform.parent)
            {
                //FIX this doesn't consider speed...
                float dmg = Damage();
                print(b + "took damage --> " + dmg);
                b.TakeDamage(dmg, dmg * 0.1f * transform.forward, Player.LocalPlayer);
            }
        }
    }


    public float Damage()
    {
        return stats.ForceBasedDamage ? stats.Damage * root.mass * owner.Speed:stats.Damage;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 position = transform.position;
        Debug.DrawRay(position, Vector3.up * stats.BaseDist);
        Debug.DrawRay(position, Vector3.forward * stats.BaseDist);
        Debug.DrawRay(position, Vector3.right * stats.BaseDist);
        Debug.DrawRay(position, Vector3.down * stats.BaseDist);
        Debug.DrawRay(position, Vector3.left * stats.BaseDist);
        Debug.DrawRay(position, Vector3.back * stats.BaseDist);
        DebugExtensions.DebugSphereCast(transform.position, transform.forward,  stats.Range.x, Color.red, stats.Range.y,0, CastDrawType.Minimal, PreviewCondition.Editor, false);
    }
    #endif

    public void Disconnect()
    {
        transform.parent = null;
        isConnected = false;
    }
}
