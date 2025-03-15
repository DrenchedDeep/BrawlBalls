using System.Collections;
using Gameplay.Abilities.WeaponAbilities;
using Managers.Local;
using RotaryHeart.Lib.PhysicsExtension;
using Stats;
using Unity.Netcode;
using UnityEngine;
using Physics = UnityEngine.Physics;

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

        

        private void Start()
        {
            
            _curDamage = stats.Damage;
#if UNITY_EDITOR
            enabled = (!NetworkManager.Singleton || IsOwner || IsServer);
            _owner = transform.parent?.GetComponent<BallPlayer>();
#else
            enabled =  IsOwner || IsServer;
#endif
            
        }

        private void LateUpdate()
        {
            if (!_owner || !_isConnected) return;
            Rotate();
        }

        private void Rotate()
        {
            Vector3 dir = Vector3.Lerp(Vector3.up,  _owner.GetBall.Velocity.normalized, _owner.GetBall.Speed * 5);
            Vector3 localDir = _owner.transform.InverseTransformDirection(dir);
             if(stats.AllowVerticalOrientation) transform.localRotation = Quaternion.LookRotation(localDir, Vector3.up);
             else transform.localRotation = Quaternion.identity;
            //transform.SetLocalPositionAndRotation(localDir, Quaternion.LookRotation(lookDirection));
        }

        private void OnTransformParentChanged()
        {
            Transform r = transform.parent;
            if (r == null) return;
            _owner = r.GetComponent<BallPlayer>();

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
