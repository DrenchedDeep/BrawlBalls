using System;
using Gameplay;
using Managers.Local;
using MarkedForDeath;
using RotaryHeart.Lib.PhysicsExtension;
using Stats;
using Unity.Netcode;
using UnityEngine;
using Physics = UnityEngine.Physics;


/**
 * this class is ONLY ran on the server, networkrigidtrans replicates the transform to other clients
 */
public class Projectile : MonoBehaviour
{
    [SerializeField] private float ballVelocityIncreaseAmt = 1;
    
    private Rigidbody _rigidbody;

    private float _damage;
    private Vector3 _initialVelocity;
    private BallPlayer _owner;
    private  ProjectileWeaponStats.ProjectileDamageType _damageType;
    private bool _isHoming;
    private bool _isAffectedByGravity;
    private float _forceMultplier;
    private LayerMask _layers;
    private float _maxRadius;
    private float _maxRange;
    
    
    private readonly RaycastHit[] _hits = new RaycastHit[10];

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Init(BallPlayer owner, ProjectileWeaponStats stats, out Vector3 velocity)
    {
        _rigidbody = GetComponent<Rigidbody>();
        Debug.Log("BALLS!");
        _initialVelocity = transform.forward * stats.InitialVelocity;
        _damage = stats.Damage;
        _owner = owner;
        _damageType = stats.DamageType;
        _isHoming = stats.IsHomingProjectile;
        _isAffectedByGravity = stats.IsAffectedByGravity;
        _layers = stats.HitLayers;
        _forceMultplier = stats.ForceMultiplier;
        _maxRadius = stats.MaxRadius;
        _maxRange = stats.MaxRange;
        if (stats.BallVelocityAffectsProjectileVelocity)
        {
            _initialVelocity += owner.GetBall.Velocity * ballVelocityIncreaseAmt;
        }
        
        _rigidbody.linearVelocity = _initialVelocity;
        velocity = _initialVelocity;
        //    Debug.Log(_initialVelocity);
    }

    private void FixedUpdate()
    {
        if (_isAffectedByGravity)
        {
            _rigidbody.AddForce(Physics.gravity * _rigidbody.mass);
        }
        switch (_damageType)
        {
            case ProjectileWeaponStats.ProjectileDamageType.Radial:
                CastForward_SphereCast();
                break;

            case ProjectileWeaponStats.ProjectileDamageType.Single:
                CastForward_Raycast();
                break;
        }
    }

    private void CastForward_Raycast()
    {
        Transform tr = transform;
        Vector3 position = tr.position;
        Vector3 forward = tr.forward;
        if (Physics.Raycast(position, forward, out RaycastHit hit, _maxRange, _layers))
        {
            Rigidbody n = hit.rigidbody;
            if (n && n.TryGetComponent(out BallPlayer b) && b != _owner)
            {
                float dmg = _damage;
                dmg *= _owner.Mass * _owner.GetBall.Speed;
                print("Doing damage: " + dmg);
                    
                DamageProperties damageProperties;
                damageProperties.Damage = Mathf.Max(0, dmg);
                damageProperties.Direction = forward * (dmg * _forceMultplier);
                damageProperties.Attacker = _owner.OwnerClientId;
                b.TakeDamage_ServerRpc(damageProperties);
            }
            Destroy(gameObject);
        }
    }

    private void CastForward_SphereCast()
    {
        Transform tr = transform;
        Vector3 position = tr.position;
        Vector3 forward = tr.forward;

        int hitCount =
            Physics.SphereCastNonAlloc(position, _maxRadius, forward, _hits, _maxRange, StaticUtilities.EnemyLayer);
        
        for (int i = 0; i < hitCount; ++i)
        {
            Rigidbody n = _hits[i].rigidbody;
            if (n && n.TryGetComponent(out BallPlayer b) && b != _owner)
            {
                //FIX this doesn't consider speed...
                float dmg = _damage;
                dmg *= _owner.Mass * _owner.GetBall.Speed;
                print("Doing damage: " + dmg);
                    
                DamageProperties damageProperties;
                damageProperties.Damage = Mathf.Max(0, dmg);
                damageProperties.Direction = forward * (dmg * _forceMultplier);
                damageProperties.Attacker = _owner.OwnerClientId;
                b.TakeDamage_ServerRpc(damageProperties);
            }
        }
    }

}
