using System.Threading;
using Cysharp.Threading.Tasks;
using Managers.Local;
using Managers.Network;
using RotaryHeart.Lib.PhysicsExtension;
using Unity.Netcode;
using UnityEngine;
using Physics = UnityEngine.Physics;

namespace Gameplay.Weapons
{
    public class TheButtonWeapon : BaseWeapon
    {
        private static readonly int IsBlowingUp = Animator.StringToHash("IsBlowingUp");
        [SerializeField] private float buttonHoldTime = 3f;
        [SerializeField] private float explosionRadius = 15;
        [SerializeField] private AnimationCurve damageFallOffCurve;
        [SerializeField] private ParticleSystem nukeParticle;
    
        private CancellationTokenSource _cancellationTokenSource;
        private Material _material;
        private Animator _animator;
        private float _currentTime;
        private bool _isHoldingDown;

        private NetworkVariable<float> crackPercent = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public override void AttackStart()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _ = ButtonCountdown(_cancellationTokenSource.Token);
            _isHoldingDown = true;
            _animator.SetBool(IsBlowingUp, true);
        }

        public override void AttackEnd()
        {
            _isHoldingDown = false;
            _cancellationTokenSource.Cancel();
            crackPercent.Value = 0;       
            _animator.SetBool(IsBlowingUp, false);

        }

        public override void Start()
        {
            base.Start();
            Owner.OnDestroyed += OnDeath;
            _material = Owner.GetBall.GetComponent<MeshRenderer>().material;
            _animator = GetComponent<Animator>();
            crackPercent.OnValueChanged += OnValueChanged;
        }

        private void OnValueChanged(float previousvalue, float newvalue)
        {
            _material.SetFloat(StaticUtilities.CrackPercentID, newvalue);
        }


        private async UniTask ButtonCountdown(CancellationToken token)
        {
            
        Debug.Log("start!");
        while (_currentTime < buttonHoldTime)
        {
            _currentTime += Time.deltaTime;
            
            crackPercent.Value = _currentTime / buttonHoldTime;
            
            await UniTask.Yield();
            
            if (token.IsCancellationRequested)
            {
                return;
            }
        }
        Explode_ServerRpc();
        }

        void OnDeath(ulong killer, int childID)
        {
            if (_isHoldingDown)
            {
                Explode_ServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void Explode_ServerRpc()
        {
            DamageProperties damageProperties = new DamageProperties(1000, transform.forward, Owner.OwnerClientId, Owner.ChildID.Value);
            Collider[] results = Physics.OverlapSphere(transform.position, explosionRadius, stats.HitLayers);

#if UNITY_EDITOR
            DebugExtensions.DebugWireSphere(transform.position, Color.red, explosionRadius, 5, PreviewCondition.Both);
#endif
        
            foreach (Collider col in results)
            {
                if (col.transform.parent && col.transform.parent.TryGetComponent(out BallPlayer ballPlayer))
                {
                    
#if UNITY_EDITOR
                    Debug.DrawLine(transform.position, col.transform.position, Color.green, 5);
#endif
                    
                    float dist = (Owner.transform.position - col.transform.position).magnitude;
                    Debug.Log("HIT: " + col.gameObject.name + " with a distance of: " + dist);
                    ballPlayer.TakeDamage_ServerRpc(damageProperties);
                }
            }
        
            //NetworkGameManager.Instance.PlayParticleGlobally_ServerRpc("Explosion", transform.position, transform.rotation);
            Owner.TakeDamage_ServerRpc(damageProperties);
            Playback_ClientRPC(transform.position);
        }

        [ClientRpc]
        private void Playback_ClientRPC(Vector3 location)
        {
            ParticleSystem ps = Instantiate(nukeParticle, location, Quaternion.identity);
            Destroy(ps.gameObject, ps.main.duration);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<BallPlayer>())
            {
                Explode_ServerRpc();
            }
        }
    }
}
