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

        public bool CanDoDamage { get; set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        //owner calls this function... they can setup velocity & the velocity is passed down to other clients
        public void Init(BallPlayer owner, out Vector3 velocity)
        {
            _rigidbody = GetComponent<Rigidbody>(); //<< This can probably be removed? That or remove awake?
            _initialVelocity = transform.forward * stats.InitialVelocity;
            _owner = owner;
            CanDoDamage = true;

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

            Destroy(gameObject, stats.MaxLifetime);
        }

        //dummy init, essentially tell it to destroy itself and to not calculate damage
        public void Init()
        {
            CanDoDamage = false;
            Destroy(gameObject, stats.MaxLifetime);
        }

        public void OverrideVelocity(Vector3 velocity) => _rigidbody.linearVelocity = velocity;

        private void FixedUpdate()
        {
            if (stats.IsAffectedByGravity)
            {
                _rigidbody.AddForce(Physics.gravity * _rigidbody.mass);
            }

            if (stats.RotateTowardsVelocity)
            {
                transform.rotation = Quaternion.LookRotation(_rigidbody.linearVelocity);
            }

            if (!stats.DoDamageAfterCollision && CanDoDamage)
            {
                _castMode.Invoke();
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (stats.DoDamageAfterCollision && CanDoDamage)
            {
                Instantiate(hitVFX, transform.position,
                    Quaternion.LookRotation(-transform.forward, Vector3.up));
                CastForward_SphereCast();
                Destroy(gameObject);
            }
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
            Vector3 pos = transform.position;
            Collider[] cols = Physics.OverlapSphere(pos, 10, StaticUtilities.PlayerLayers);
            foreach (Collider c in cols)
            {
                Vector3 ePos = c.ClosestPoint(pos);
                Vector3 dir = ePos - pos;
                float damage = ParticleManager.EvalauteExplosiveDistance(dir.magnitude / 10) * 200;

                DamageProperties damageProperties;
                damageProperties.Damage = damage;
                damageProperties.Direction = damage * dir;
                damageProperties.Attacker = _owner.OwnerClientId;


                c.attachedRigidbody.GetComponent<BallPlayer>().TakeDamage_ServerRpc(damageProperties);
            }

        }
    }
}
