using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Abilities.WeaponAbilities;
using RotaryHeart.Lib.PhysicsExtension;
using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class BaseWeapon : NetworkBehaviour
    {
        [SerializeField] protected WeaponStats stats;
        [SerializeField] private bool blockVerticalOrientation;
        [SerializeField] private bool lookUpWhileNotMoving;
        
        protected float CurDamage;
        
        protected BallPlayer Owner;
        private Transform _connector;
        protected bool IsConnected = true;
    
        public bool IsChargingUp { get; private set; }


        public WeaponStats Stats => stats;
        public AbilityStats GetAbility => stats.Ability;
        
        private readonly CancellationTokenSource _chargeUpCancelToken = new();

        protected float _currentChargeUp;
        private bool _canDrop;

        public virtual void Start()
        {
            
            CurDamage = stats.Damage;
            /*/
#if UNITY_EDITOR
            enabled = (!NetworkManager.Singleton || IsOwner || IsServer);
            _owner = transform.parent?.GetComponent<BallPlayer>();
#else
            enabled =  IsOwner || IsServer;
#endif
/*/
            
            Owner = transform.parent?.GetComponent<BallPlayer>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                _canDrop = false;
                _ = CanDropDelay();
            }
        }

        protected virtual void LateUpdate()
        {
            if (!IsConnected || !IsOwner) return;
            Rotate();
        }

        private async UniTask CanDropDelay()
        {
            await UniTask.WaitForSeconds(3);
            _canDrop = true;
        }

        private void Rotate()
        {
            float s = Owner.GetBall.Speed;
            if (s < 0.02f) return;
            
            Vector3 dir = Owner.GetBall.Velocity;
            
            if (blockVerticalOrientation) dir.y = 0;
            
            dir.Normalize();
            
            if (lookUpWhileNotMoving) dir = Vector3.Lerp(Vector3.up,  dir, s * 5);
            
         //   Debug.Log(dir);
            transform.forward = dir;
        }

        private void OnTransformParentChanged()
        {
            Transform r = transform.parent;
            if (r == null) return;
            Owner = r.GetComponent<BallPlayer>();

        }



        #region Input
        public void AttackStart()
        {
            Debug.Log("Trying to attack: " + CanAttack());
            if (!CanAttack()) return;
            _ = AttackChargeUp();
        }
        public void AttackEnd()
        {
            if (stats.FireOnRelease)
            {
                _currentChargeUp = 0;
                OnChargeStop();
                Attack();
            }
            else if (IsChargingUp)
            {
                _chargeUpCancelToken.Cancel();
               
                OnChargeStop();
            }
            IsChargingUp = false;
        }

        public virtual bool CanAttack()
        {
            return !IsChargingUp;
        }
        #endregion

   

        private async UniTask AttackChargeUp()
        {
            IsChargingUp = true;
            Debug.Log("Beginning charge up");

            OnChargeStart();
            
            while (_currentChargeUp < Stats.ChargeUpTime)
            {
                float dt = Time.deltaTime;
                _currentChargeUp += dt;
                ChargeUpTick(dt);
                await UniTask.Yield();
            }

            if (stats.FireOnRelease)
                return;
            
            _currentChargeUp = 0;
            IsChargingUp = false;
            Debug.Log("Executing attack");

            OnChargeStop();
            
            Attack();
        }

        protected virtual void Attack() { }
        protected virtual void ChargeUpTick(float deltaTime) { }
        protected virtual void OnChargeStart() { }
        protected virtual void OnChargeStop() { }
        



        

        
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            DebugExtensions.DebugSphereCast(transform.position, transform.forward,  stats.MaxRange, Color.green, stats.MaxRadius,0, CastDrawType.Minimal, PreviewCondition.Editor, true);
        }
        #endif
        
        
        [ServerRpc(RequireOwnership = false)]
        private void Disconnect_ServerRpc(float speed)
        {
            NetworkObject.TryRemoveParent();
            DisconnectClientRpc();
            Owner.StartCoroutine(Spike.Move(this , speed * 5)); // Owner is just the object running the coroutine
        }

        [ClientRpc]
        private void DisconnectClientRpc()
        {
            gameObject.layer = 0;
            IsConnected = false;
        }

        public void Disconnect(float speed)
        {
            if (_canDrop)
            {
                Disconnect_ServerRpc(speed);
            }
        }
    }
}
