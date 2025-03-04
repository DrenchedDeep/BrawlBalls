using System;
using Gameplay.Abilities.WeaponAbilities;
using Managers.Local;
using RotaryHeart.Lib.PhysicsExtension;
using Stats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using Physics = UnityEngine.Physics;

namespace Gameplay
{
    public class Weapon : NetworkBehaviour
    {
    
        [SerializeField] private WeaponStats stats;

        public readonly RaycastHit[] Hits = new RaycastHit[10];
        public int HitCount { get; private set; }

        public float Mass => stats.Mass;
        public Vector2 Range => stats.Range;
        private BallPlayer _owner;
        public AbilityStats GetAbility => stats.Ability;

        private float _curDamage;
        private bool isConnected;

        private Transform connector;
    
    

        [SerializeField] private bool isActive = true;
        
        public void ToggleActive()
        {
            isActive = !isActive && IsHost;
        }


        private void Start()
        {
            
            _curDamage = stats.Damage;
                    
            isActive = IsOwner || IsServer;

        }

        private void LateUpdate()
        {
            if (!isConnected) return;
            Rotate();
        }

        private void Rotate()
        {
            Vector3 dir = Vector3.Lerp(Vector3.up,  _owner.GetBall.Velocity.normalized, _owner.GetBall.Speed * 5);
           // Debug.Log("Lerping from: " + _owner.GetBall.Velocity.normalized + " to " + Vector3.up + " Where T is " + _owner.GetBall.Speed);
            
            Vector3 localDir = _owner.transform.InverseTransformDirection(dir);
            
            transform.SetLocalPositionAndRotation(localDir * stats.BaseDist, Quaternion.LookRotation(localDir));
            //transform.SetLocalPositionAndRotation();
        }

        private void OnTransformParentChanged()
        {
            Transform r = transform.parent;
            if (r == null) return;
            _owner = r.GetComponent<BallPlayer>();
            isConnected = true;

        }

        //Default update, always check forward, and if hitting enemy then do thing...
        private void FixedUpdate()
        {
            if(isActive) //This is inheriently an owner only call.
                CastForward();
            //if(IsOwner && _isConnected)
            //Rotate();
            
        }
        

        //The server should just process this?
        private void CastForward()
        {
            Vector3 position = transform.position;
            Vector3 forward = transform.forward;
        
            float dist = Physics.Raycast(position, forward, out RaycastHit wallCheck,  stats.Range.x, StaticUtilities.GroundLayers)?wallCheck.distance:stats.Range.x;

            HitCount = Physics.SphereCastNonAlloc(position, stats.Range.y, forward, Hits, dist, stats.HitLayers);
            
            for (int i = 0; i < HitCount; ++i)
            {
                Rigidbody n = Hits[i].rigidbody;
                if (n && n.TryGetComponent(out BallPlayer b) && b != _owner)
                {
                    //FIX this doesn't consider speed...
                    float dmg = _curDamage;
                    if (stats.ForceBasedDamage) dmg *= _owner.Mass * _owner.GetBall.Speed;
                    print("Doing damage: " + dmg);
                    b.TakeDamage_ClientRpc(Mathf.Max(0,dmg), forward * (dmg * stats.PushMul), OwnerClientId);
                }
            }
        }



#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 position = transform.position;
            DebugExtensions.DebugSphereCast(transform.position, transform.forward,  stats.Range.x, Color.green, stats.Range.y,0, CastDrawType.Minimal, PreviewCondition.Editor, true);
            Debug.DrawRay(position, Vector3.up * stats.BaseDist);
            Debug.DrawRay(position, Vector3.forward * stats.BaseDist);
            Debug.DrawRay(position, Vector3.right * stats.BaseDist);
            Debug.DrawRay(position, Vector3.down * stats.BaseDist);
            Debug.DrawRay(position, Vector3.left * stats.BaseDist);
            Debug.DrawRay(position, Vector3.back * stats.BaseDist);
        }
#endif

        [ServerRpc(RequireOwnership = false)]
        private void DisconnectServerRpc(float speed)
        {
            NetworkObject.TryRemoveParent();
            DisconnectClientRpc();
            _owner.StartCoroutine(Spike.Move(this , speed * 5)); // Owner is just the object running the coroutine
        }

        [ClientRpc]
        private void DisconnectClientRpc()
        {
            gameObject.layer = 0;
            GetComponent<BoxCollider>().enabled = true;
            isConnected = false;
            if (!IsHost)
            {
                enabled = false;
            }
        }

        public void Disconnect(float speed)
        {
            DisconnectServerRpc(speed);
        
        }
    
        public float MultiplyDamage(int i)
        {
            _curDamage *= i;
            return _curDamage;
        }
    }
}
