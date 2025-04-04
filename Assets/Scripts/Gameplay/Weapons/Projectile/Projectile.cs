using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Balls;
using Gameplay.Pools;
using Managers.Local;
using RotaryHeart.Lib.PhysicsExtension;
using Stats;
using UnityEngine;
using Physics = UnityEngine.Physics;


namespace Gameplay.Weapons
{
    /**
 * this class is ONLY ran on the server, networkrigidtrans replicates the transform to other clients
 */
    public class Projectile : PooledObject
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

        private MeshRenderer[] _renderers;
        private ParticleSystem[] _particleSystems;
        private TrailRenderer _trailRenderer;
        

        public bool CanDoDamage { get; set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
            _trailRenderer = GetComponentInChildren<TrailRenderer>();
            _renderers = GetComponentsInChildren<MeshRenderer>();

            _rigidbody.useGravity = false;
        }


        public void Init(BallPlayer owner, Vector3 direction, float multSpeed = 1)
        {
            transform.forward = direction;
            Init(owner, multSpeed);
        }

        //owner calls this function... they can setup velocity & the velocity is passed down to other clients
        public void Init(BallPlayer owner, float multSpeed = 1)
        {
            enabled = true;
            
            _initialVelocity = transform.forward * (stats.InitialVelocity*multSpeed);
            _owner = owner;
            CanDoDamage = true;

            if (stats.BallVelocityAffectsProjectileVelocity)
            {
                _initialVelocity += owner.GetBall.Velocity * ballVelocityIncreaseAmt;
            }

            _rigidbody.isKinematic = false;
            _rigidbody.linearVelocity = _initialVelocity;

            _castMode = stats.DamageType switch
            {
                ProjectileStats.ProjectileDamageType.Radial => CastForward_SphereCast,
                ProjectileStats.ProjectileDamageType.Single => CastForward_Raycast,
                _ => throw new ArgumentOutOfRangeException() // Default.
            };

            PoolCancellation = new CancellationTokenSource();

            _ = ReturnToPoolTask(PoolCancellation, stats.MaxLifetime);

        }

        //dummy init, essentially tell it to destroy itself and to not calculate damage
        public void Init()
        {
            CanDoDamage = false;
            PoolCancellation = new CancellationTokenSource();
            _ = ReturnToPoolTask(PoolCancellation, stats.MaxLifetime);
        }
        

        public void OverrideVelocity(Vector3 directionVector, bool useInitialVelocity=true, bool faceVelocity = true)
        {
            _rigidbody.isKinematic = false;
            _rigidbody.linearVelocity = directionVector * (useInitialVelocity? stats.InitialVelocity:1);
            if(faceVelocity) transform.forward = directionVector;

        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(Physics.gravity * (stats.GravMult * Time.fixedDeltaTime), ForceMode.Force);
            
            
            if (stats.RotateTowardsVelocity)
            {
                transform.rotation = Quaternion.LookRotation(_rigidbody.linearVelocity);
            }

            if (!stats.DoDamageAfterCollision && CanDoDamage)
            {
                _castMode.Invoke();
            }
        }
        public override async void ReturnToPool()
        {
            _rigidbody.isKinematic = true;
            foreach (var ps in _particleSystems)
            {
                ps.Stop();
            }

            foreach (var mr in _renderers)
            {
                mr.gameObject.SetActive(false);
            }
            if(_trailRenderer) _trailRenderer.emitting = false;
            await UniTask.WaitForSeconds(stats.EffectDisableTime);

            base.ReturnToPool(); 
            enabled = false;
        }

        private void OnEnable()
        {
             //we must not be initialiazed
            _rigidbody.isKinematic = false;
            foreach (var ps in _particleSystems)
            {
                ps.Play();
            }

            foreach (var mr in _renderers)
            {
                mr.gameObject.SetActive(true);
            }
            if(_trailRenderer) _trailRenderer.emitting = true;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (stats.DoDamageAfterCollision)
            {
                if (CanDoDamage)
                {
                    CastForward_SphereCast();
                }

                ObjectPoolManager.Instance.GetObjectFromPool<PooledParticle>
                    (stats.HitVfxPoolName, transform.position, Quaternion.LookRotation(-transform.forward, Vector3.up));

                ReturnToPool();
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
                    DamageProperties damageProperties;
                    damageProperties.Damage = Mathf.Max(0, dmg);
                    damageProperties.Direction = forward * (dmg * stats.ForceMultiplier);
                    damageProperties.Attacker = _owner.OwnerClientId;
                    damageProperties.ChildID = _owner.Owner.PlayerInput.playerIndex;
                    b.TakeDamage_ServerRpc(damageProperties);
                }

                ObjectPoolManager.Instance.GetObjectFromPool<PooledParticle>
                    (stats.HitVfxPoolName, transform.position, Quaternion.LookRotation(-transform.forward, Vector3.up));
                ReturnToPool();
            }
        }

        private void CastForward_SphereCast()
        {
            Vector3 pos = transform.position;
            Collider[] cols = Physics.OverlapSphere(pos, stats.MaxRadius, StaticUtilities.PlayerLayers);
            

            foreach (Collider c in cols)
            {
                Vector3 ePos = c.ClosestPoint(pos);
                Vector3 dir = ePos - pos;
                float damage = ParticleManager.EvalauteExplosiveDistance(dir.magnitude / 100) * stats.Damage;
                Debug.Log("percent for damage:" + dir.magnitude / 100);
                DamageProperties damageProperties;
                damageProperties.Damage = damage;
                damageProperties.Direction = damage * dir;
                damageProperties.Attacker = _owner.OwnerClientId;
                damageProperties.ChildID = _owner.Owner.PlayerInput.playerIndex;

                Rigidbody rb = c.attachedRigidbody;
                if (rb &&rb.TryGetComponent(out BallPlayer bp))
                {
                    bp.TakeDamage_ServerRpc(damageProperties);
                }
            }
        }
    }
}
