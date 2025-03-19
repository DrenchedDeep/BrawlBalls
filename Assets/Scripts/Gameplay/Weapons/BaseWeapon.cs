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
        


        protected float CurDamage;
        
        protected BallPlayer Owner;
        private Transform _connector;
        protected bool IsConnected = true;
    

        public WeaponStats Stats => stats;
        public AbilityStats GetAbility => stats.Ability;

        

        private IWeaponComponent[] _weaponComponents;

        public void Start()
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
            _weaponComponents = GetComponentsInChildren<IWeaponComponent>();

            foreach (IWeaponComponent comp in _weaponComponents)
            {
                comp.Init(Owner);
            }
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
            
            transform.forward = dir;
        }

        private void OnTransformParentChanged()
        {
            Transform r = transform.parent;
            if (r == null) return;
            Owner = r.GetComponent<BallPlayer>();

        }

        
        
        public virtual void Attack() { }
        
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
