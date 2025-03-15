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
            if (!_owner) return;
            Rotate();
        }

        private void Rotate()
        {
            Vector3 dir = Vector3.Lerp(Vector3.up,  _owner.GetBall.Velocity.normalized, _owner.GetBall.Speed * 5);
            Vector3 localDir = _owner.transform.InverseTransformDirection(dir);
            Vector3 lookDirection = stats.AllowVerticalOrientation ? localDir : new Vector3(localDir.x, 0, localDir.z);
            transform.localRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
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
    }
}
