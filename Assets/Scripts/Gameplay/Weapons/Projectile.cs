using System;
using Managers.Local;
using Stats;
using UnityEngine;
using Physics = UnityEngine.Physics;


namespace Gameplay.Weapons
{
    /**
 * this class is ONLY ran on the server, networkrigidtrans replicates the transform to other clients
 */
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float ballVelocityIncreaseAmt = 1;
        [SerializeField] private GameObject hitVFX;
        [SerializeField] private ProjectileStats stats;
        
        private BallPlayer _owner;
        private Rigidbody _rigidbody;
        private Vector3 _initialVelocity;

        private delegate void CastMode();
        private CastMode _castMode;
    
    
        private readonly RaycastHit[] _hits = new RaycastHit[10];

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void InitSimple(Vector3 velocity)
        {
            if (!_rigidbody)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            _rigidbody.linearVelocity = velocity;
        }

        public void Init(BallPlayer owner, out Vector3 velocity)
        {
            _rigidbody = GetComponent<Rigidbody>(); //<< This can probably be removed? That or remove awake?
            _initialVelocity = transform.forward * stats.InitialVelocity;
            _owner = owner;
            
            if (stats.BallVelocityAffectsProjectileVelocity)
            {
                _initialVelocity += owner.GetBall.Velocity * ballVelocityIncreaseAmt;
            }
        
            _rigidbody.linearVelocity = _initialVelocity;
            velocity = _initialVelocity;
            
            _castMode = stats.DamageType switch
            {
                ProjectileStats.ProjectileDamageType.Radial => CastForward_SphereCast,
                ProjectileStats.ProjectileDamageType.Single => CastForward_Raycast,
                _ => throw new ArgumentOutOfRangeException() // Default.
            };
        }

        private void FixedUpdate()
        {
            if (stats.IsAffectedByGravity)
            {
                _rigidbody.AddForce(Physics.gravity * _rigidbody.mass);
            }
            _castMode.Invoke();
        }

        private void CastForward_Raycast()
        {
            Transform tr = transform;
            Vector3 position = tr.position;
            Vector3 forward = tr.forward;
            if (Physics.Raycast(position, forward, out RaycastHit hit, stats.MaxRange, stats.HitLayers))
            {
                Rigidbody n = hit.rigidbody;
                if (n && n.TryGetComponent(out BallPlayer b) && b != _owner)
                {
                    float dmg = stats.Damage;
                    dmg *= _owner.Mass * _owner.GetBall.Speed;
                    print("Doing damage: " + dmg);
                    
                    DamageProperties damageProperties;
                    damageProperties.Damage = Mathf.Max(0, dmg);
                    damageProperties.Direction = forward * (dmg * stats.ForceMultiplier);
                    damageProperties.Attacker = _owner.OwnerClientId;
                    b.TakeDamage_ServerRpc(damageProperties);
                }
                GameObject hitVfx = Instantiate(hitVFX, hit.point, Quaternion.LookRotation(-forward, Vector3.up));
                Debug.Log("projectile hit: " + hit.transform.gameObject.name);
                Destroy(gameObject);
            }
        }

        private void CastForward_SphereCast()
        {
            Transform tr = transform;
            Vector3 position = tr.position;
            Vector3 forward = tr.forward;

            int hitCount =
                Physics.SphereCastNonAlloc(position, stats.MaxRadius, forward, _hits, stats.MaxRange, StaticUtilities.EnemyLayer);
        
            for (int i = 0; i < hitCount; ++i)
            {
                Rigidbody n = _hits[i].rigidbody;
                if (n && n.TryGetComponent(out BallPlayer b) && b != _owner)
                {
                    //FIX this doesn't consider speed...
                    float dmg = stats.Damage;
                    dmg *= _owner.Mass * _owner.GetBall.Speed;
                    print("Doing damage: " + dmg);
                    
                    DamageProperties damageProperties;
                    damageProperties.Damage = Mathf.Max(0, dmg);
                    damageProperties.Direction = forward * (dmg * stats.ForceMultiplier);
                    damageProperties.Attacker = _owner.OwnerClientId;
                    b.TakeDamage_ServerRpc(damageProperties);
                }
            }
        }

    }
}
