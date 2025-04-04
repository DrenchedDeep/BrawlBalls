using Cysharp.Threading.Tasks;
using Managers.Local;
using RotaryHeart.Lib.PhysicsExtension;
using UnityEngine;
using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;

namespace Gameplay.Weapons
{
    public class MeleeWeapon : BaseWeapon
    {
        private static readonly int ProgressID = Shader.PropertyToID("_Progress");
        [SerializeField] private float damageTickRate = 0.2f;
        
        public readonly RaycastHit[] Hits = new RaycastHit[10];

        private float _currentTime;

        [SerializeField] private MeshRenderer speedRenderer;
        private Material _material;

        public override void Start()
        {
            base.Start();
            _material = speedRenderer.material;
            _ = CastForwardTask();
        }

        private async UniTask CastForwardTask()
        {
            float currentTime = 0;
            while (true)
            {
                currentTime += Time.deltaTime;

                if (currentTime >= damageTickRate)
                {
                    CastForward();
                    currentTime = 0;
                }

                await UniTask.Yield();
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            _material.SetFloat(ProgressID, Owner.GetBall.Speed / Owner.GetBall.MaxSpeed);
        }

        //The server should just process this?
        private void CastForward()
        {
            Transform tr = transform;
            Vector3 position = tr.position;
            Vector3 forward = tr.forward;

            bool hitWall = Physics.Raycast(position, forward, out RaycastHit wallCheck, stats.MaxRange, StaticUtilities.GroundLayers);
            float dist = hitWall?wallCheck.distance:stats.MaxRange;

            int hitCount = Physics.SphereCastNonAlloc(position, stats.MaxRadius, forward, Hits, dist,
                stats.HitLayers, PreviewCondition.Editor, 0.1f, Color.green, Color.red);
            
            for (int i = 0; i < hitCount; ++i)
            {
                Rigidbody n = Hits[i].rigidbody;
                n.TryGetComponent(out BallPlayer b);
                if (n && CanDamage(b))
                {
                    //FIX this doesn't consider speed...
                    float dmg = CurDamage;
                    dmg *= Owner.Mass * Owner.GetBall.Speed * 0.001f;
                    print("spike Doing damage: " + dmg);
                    
                    DamageProperties damageProperties;
                    damageProperties.Damage = Mathf.Max(0, dmg);
                    damageProperties.Direction = forward * (dmg * stats.ForceMultiplier);
                    damageProperties.Attacker = OwnerClientId;
                    damageProperties.ChildID = Owner.ChildID.Value;
                    b.TakeDamage_ServerRpc(damageProperties);
                }
            }

            if (hitWall && !IsConnected)
            {
                enabled = false;
            }
        }

        // in theory this should work :P
        private bool CanDamage(BallPlayer b)
        {
            if (b.OwnerClientId == Owner.OwnerClientId)
            {
                return b.ChildID.Value != Owner.ChildID.Value;
            }
            
            return true;
        }
    }
}