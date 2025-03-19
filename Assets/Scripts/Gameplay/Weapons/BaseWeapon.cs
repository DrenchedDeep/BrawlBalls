using System.Collections.Generic;
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
        


        protected float _curDamage;
        
        protected BallPlayer _owner;
        private Transform _connector;
        protected bool _isConnected = true;
    

        public WeaponStats Stats => stats;
        public AbilityStats GetAbility => stats.Ability;

        

        private IWeaponComponent[] _weaponComponents;

        private void Start()
        {
            
            _curDamage = stats.Damage;
#if UNITY_EDITOR
            enabled = (!NetworkManager.Singleton || IsOwner || IsServer);
            _owner = transform.parent?.GetComponent<BallPlayer>();
#else
            enabled =  IsOwner || IsServer;
#endif

            _weaponComponents = GetComponentsInChildren<IWeaponComponent>();

            foreach (IWeaponComponent comp in _weaponComponents)
            {
                comp.Init(_owner);
            }
        }

        private void LateUpdate()
        {
            if (!_owner || !_isConnected) return;
            Rotate();
        }

        private void Rotate()
        {
            Vector3 dir = _owner.GetBall.Velocity;
            
            if (blockVerticalOrientation) dir.y = 0;
            
            dir.Normalize();
            
            if (lookUpWhileNotMoving) dir = Vector3.Lerp(Vector3.up,  dir, _owner.GetBall.Speed * 5);
            
            transform.forward = dir;
             Debug.DrawRay(transform.position, dir * 4, Color.red);
        }

        private void OnTransformParentChanged()
        {
            Transform r = transform.parent;
            if (r == null) return;
            _owner = r.GetComponent<BallPlayer>();

        }

        
        public void Attack()
        {
            for (int i = 0; i < _weaponComponents.Length; i++)
            {
                _weaponComponents[i].Fire(stats, out Vector3 velocity);
                
                if (NetworkManager.Singleton)
                {
                    Attack_ServerRpc(i, velocity);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void Attack_ServerRpc(int index, Vector3 velocity) => Attack_ClientRpc(index, velocity);

        [ClientRpc(RequireOwnership = false)]
        void Attack_ClientRpc(int index, Vector3 velocity)
        {
            if (!IsOwner)
            {
                _weaponComponents[index].FireDummy(stats, velocity);
            }
        }



    
        public float MultiplyDamage(int i)
        {
            _curDamage *= i;
            return _curDamage;
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
            _owner.StartCoroutine(Spike.Move(this , speed * 5)); // Owner is just the object running the coroutine
        }

        [ClientRpc]
        private void DisconnectClientRpc()
        {
            gameObject.layer = 0;
            _isConnected = false;
        }

        public void Disconnect(float speed)
        {
            Disconnect_ServerRpc(speed);
        }
    }
}
