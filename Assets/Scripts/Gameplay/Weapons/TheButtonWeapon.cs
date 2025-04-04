using System.Threading;
using Cysharp.Threading.Tasks;
using Managers.Local;
using RotaryHeart.Lib.PhysicsExtension;
using Unity.Netcode;
using UnityEngine;
using Physics = UnityEngine.Physics;

namespace Gameplay.Weapons
{
    public class TheButtonWeapon : BaseWeapon
    {
        private static readonly int IsBlowingUp = Animator.StringToHash("IsBlowingUp");
        [SerializeField] private AnimationCurve damageFallOffCurve;
        [SerializeField] private ParticleSystem nukeParticle;
    
        private CancellationTokenSource _cancellationTokenSource;
        private Material _material;
        private Animator _animator;
        private float _currentTime;
        private bool _isHoldingDown;
        private bool _isLockedDown;

        private readonly NetworkVariable<float> _crackPercent = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        protected override void OnChargeStart()
        {
            Debug.Log("Nuclear button Pressed");
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _ = ButtonCountdown(_cancellationTokenSource.Token);
            
            _isHoldingDown = true;
            _animator.SetBool(IsBlowingUp, true);
        }

        protected override void OnChargeStop()
        {
            if (_isLockedDown) return;
            
            Debug.Log("Nuclear button unpressed");

            
            _isHoldingDown = false;
            _crackPercent.Value = 0;       
            _animator.SetBool(IsBlowingUp, false);

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _ = ButtonUndo(_cancellationTokenSource.Token);
        }

        public override void Start()
        {
            base.Start();
            Owner.OnDestroyed += OnDeath;
            _material = Owner.GetBall.GetComponent<MeshRenderer>().material;
            _animator = GetComponent<Animator>();
            _crackPercent.OnValueChanged += OnValueChanged;
        }

        private void OnValueChanged(float previousvalue, float newvalue)
        {
            _material.SetFloat(StaticUtilities.CrackPercentID, newvalue);
        }


        private async UniTask ButtonCountdown(CancellationToken token)
        {
            
        while (_currentTime < stats.ChargeUpTime)
        {
            _currentTime += Time.deltaTime;

            _crackPercent.Value = _currentTime / stats.ChargeUpTime;

            await UniTask.Yield();

            if (token.IsCancellationRequested)
            {
                return;
            }
        }
        _animator.SetBool(IsBlowingUp, false);
        _isHoldingDown = false;
        _isLockedDown = false;
        Explode_ServerRpc();
        }

        private async UniTask ButtonUndo(CancellationToken token)
        {
            while (_currentTime >0 )
            {
                _currentTime -= Time.deltaTime;

                _crackPercent.Value = _currentTime / stats.ChargeUpTime;

                await UniTask.Yield();

                if (token.IsCancellationRequested)
                {
                    return;
                }
            }

            _currentTime = 0;
        }

        void OnDeath(ulong killer, int childID)
        {
            if (_isHoldingDown)
            {
                _animator.SetBool(IsBlowingUp, false);
                Explode_ServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void Explode_ServerRpc()
        {
            Debug.Log("Nuclear Explosion");

            
            Playback_ClientRPC(transform.position);

            Collider[] results = Physics.OverlapSphere(transform.position, stats.MaxRadius, stats.HitLayers);

#if UNITY_EDITOR
            DebugExtensions.DebugWireSphere(transform.position, Color.red, stats.MaxRadius, 5, PreviewCondition.Both);
#endif
        
            foreach (Collider col in results)
            {
                if (col.transform.parent && col.transform.parent.TryGetComponent(out BallPlayer ballPlayer))
                {
                    

                    Vector3 direction = col.transform.position - Owner.transform.position;
                    float dist = (direction).magnitude;
                    Debug.Log("HIT: " + col.gameObject.name + " with a distance of: " + dist);
                    
#if UNITY_EDITOR
                    Debug.DrawLine(transform.position,  direction.normalized * dist, Color.green, 5);
#endif
                    float eval = 1 - damageFallOffCurve.Evaluate(dist / stats.MaxRadius);
                    ballPlayer.TakeDamage_ServerRpc( new DamageProperties( eval * stats.Damage, direction * (eval * stats.ForceMultiplier), Owner.OwnerClientId, Owner.ChildID.Value));
                }
            }
        
            //NetworkGameManager.Instance.PlayParticleGlobally_ServerRpc("Explosion", transform.position, transform.rotation);
            Owner.TakeDamage_ServerRpc(new DamageProperties( stats.Damage, Vector3.up * stats.ForceMultiplier, Owner.OwnerClientId, Owner.ChildID.Value));
        }

        [ClientRpc]
        private void Playback_ClientRPC(Vector3 location)
        {
            Debug.Log("Playing Nuclear Particle");
            ParticleSystem ps = Instantiate(nukeParticle, location, Quaternion.Euler(-90,0,0));
            Destroy(ps.gameObject, ps.main.duration);
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb && rb.TryGetComponent(out BallPlayer b) && transform.parent != rb.transform)
            {
                Debug.Log("Something has collided with me!");
                _isLockedDown = true;
                AttackStart();
            }
        }
        
        
    }
}
