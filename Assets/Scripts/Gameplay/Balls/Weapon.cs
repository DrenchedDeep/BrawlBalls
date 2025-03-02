using Gameplay.Abilities.WeaponAbilities;
using Managers;
using RotaryHeart.Lib.PhysicsExtension;
using Stats;
using Unity.Netcode;
using UnityEngine;
using Physics = UnityEngine.Physics;

namespace Gameplay.Balls
{
    public class Weapon : NetworkBehaviour
    {
    
        [SerializeField] private WeaponStats stats;

        public readonly RaycastHit[] Hits = new RaycastHit[10];
        public int HitCount { get; private set; }

        public float Mass => stats.Mass;
        public Vector2 Range => stats.Range;
        private NetworkBall _owner;
        private Rigidbody _root;
        private bool _isConnected = true;
        public AbilityStats GetAbility => stats.Ability;
        private Transform _connector;

        private float _curDamage;
    
    

        [SerializeField] private bool isActive = true;
        public void ToggleActive()
        {
            isActive = !isActive && IsHost;
        }
        private void OnTransformParentChanged()
        {
            if (transform.parent == null || !GameManager.GameStarted) return;
            _owner = transform.parent.GetComponent<NetworkBall>();
            _connector = _owner.transform.GetChild(1);
            _root = _connector.GetComponent<Rigidbody>();
            _curDamage = stats.Damage;
            enabled = true; 
            NetworkObject.enabled = true;
        
            if (!IsHost)
                isActive = false;
        }

        //Default update, always check forward, and if hitting enemy then do thing...
        private void FixedUpdate()
        {
            if(isActive) //This is inheriently an owner only call.
                CastForward();
            //if(IsOwner && _isConnected)
            //Rotate();
        }

    
        private void LateUpdate()
        {
            if(_isConnected)
                Rotate();
        }

    
        private void Rotate()
        {
            Vector3 dir = Vector3.Lerp(Vector3.up, _owner.Velocity.normalized, _owner.Speed);
            transform.position = _connector.position + dir * stats.BaseDist;
            transform.forward = dir;
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

                Transform n = Hits[i].transform.parent;

                if (n && n.TryGetComponent(out NetworkBall b) && n != transform.parent)
                {
                    //FIX this doesn't consider speed...
                    float dmg = _curDamage;
                    if (stats.ForceBasedDamage)
                        dmg *= _root.mass * _owner.Speed;
                    print("Doing damage: " + dmg);
                    b.TakeDamageClientRpc(Mathf.Max(0,dmg), forward * (dmg * stats.PushMul), OwnerClientId);
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
            _isConnected = false;
            _owner.StartCoroutine(Spike.Move(this , speed * 5)); // Owner is just the object running the coroutine
        }

        [ClientRpc]
        private void DisconnectClientRpc()
        {
            gameObject.layer = 0;
            GetComponent<BoxCollider>().enabled = true;
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
