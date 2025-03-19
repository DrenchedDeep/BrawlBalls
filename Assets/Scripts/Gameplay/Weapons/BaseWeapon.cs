using System.Collections.Generic;
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

        
        public readonly RaycastHit[] Hits = new RaycastHit[10];


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
            
            if (stats.BlockVerticalOrientation) dir.y = 0;
            
            dir.Normalize();
            
            if (stats.LookUpWhileNotMoving) dir = Vector3.Lerp(Vector3.up,  dir, _owner.GetBall.Speed * 5);
            
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
            if (NetworkManager.Singleton)
            {
                Attack_ServerRpc();
            }
            else
            {
                foreach (IWeaponComponent comp in _weaponComponents)
                {
                    Debug.Log("attack comp");

                    comp.Fire(stats);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void Attack_ServerRpc()
        {
            Debug.Log("attack server");

            foreach (IWeaponComponent comp in _weaponComponents)
            {
                Debug.Log("attack server comp");

                comp.Fire(stats);
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
