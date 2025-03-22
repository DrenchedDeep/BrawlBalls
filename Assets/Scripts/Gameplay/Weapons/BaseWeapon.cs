using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Abilities.WeaponAbilities;
using RotaryHeart.Lib.PhysicsExtension;
using Stats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

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
        public bool IsRecharging { get; private set; }

        public WeaponStats Stats => stats;
        public AbilityStats GetAbility => stats.Ability;
        
        private readonly CancellationTokenSource _chargeUpCancelToken = new();
        private readonly CancellationTokenSource _rechargeCancelToken = new();

        private int _currentFireCount;


        public virtual void Start()
        {
            
            CurDamage = stats.Damage;
            _currentFireCount = stats.Ammo;
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

        private void LateUpdate()
        {
            if (!IsConnected) return;
            Rotate();
        }

        private void Rotate()
        {
            Vector3 dir = Owner.GetBall.Velocity;
            
            if (blockVerticalOrientation) dir.y = 0;
            
            dir.Normalize();
            
            if (lookUpWhileNotMoving) dir = Vector3.Lerp(Vector3.up,  dir, Owner.GetBall.Speed * 5);
            
            Debug.Log(dir);
            transform.forward = dir;
        }

        private void OnTransformParentChanged()
        {
            Transform r = transform.parent;
            if (r == null) return;
            Owner = r.GetComponent<BallPlayer>();

        }



        public virtual void AttackStart()
        {
            if (_currentFireCount <= 0 || IsRecharging)
            {
                return;
            }
            
            if (Stats.ChargeUpTime > 0)
            {
                if (!IsChargingUp)
                {
                    _ = AttackChargeUp();
                }
            }
            else
            {
                Attack();
            }
        }

        protected virtual void Attack()
        {
            _currentFireCount--;
        }

        public virtual  void AttackEnd()
        {
            if (IsChargingUp)
            {
                _chargeUpCancelToken.Cancel();
                IsChargingUp = false;
            }
            
            if (!IsRecharging)
            {
                _ = AttackReCharge(_rechargeCancelToken.Token);
            }

        }

        private async UniTask AttackChargeUp()
        {
            Debug.Log("uni task charge up");
            IsChargingUp = true;
            await UniTask.WaitForSeconds(stats.ChargeUpTime);
            Debug.Log("uni task charge up 2");

            Attack();

            /*/
            if (!token.IsCancellationRequested)
            {
                Attack();
                Debug.Log("charge up complete");
            }
            /*/

            IsChargingUp = false;
        }
        
        private async UniTask AttackReCharge(CancellationToken token)
        {
            IsRecharging = true;
            while (_currentFireCount < stats.Ammo)
            {
                _currentFireCount++;
                Debug.Log("fire count: " + _currentFireCount);
                await UniTask.WaitForSeconds(stats.ChargeUpTime, cancellationToken: token);
            }

            IsRecharging = false;
        }
        
        public float GetAmmoPercentage() => _currentFireCount / (float)stats.Ammo;

        
        public float MultiplyDamage(int i)
        {
            CurDamage *= i;
            return CurDamage;
        }
        
        
        
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
            Disconnect_ServerRpc(speed);
        }
    }
}
