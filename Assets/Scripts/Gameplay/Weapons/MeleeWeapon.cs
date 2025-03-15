using Managers.Local;
using RotaryHeart.Lib.PhysicsExtension;
using UnityEngine;
using Physics = RotaryHeart.Lib.PhysicsExtension.Physics;

namespace Gameplay.Weapons
{
    public class MeleeWeapon : BaseWeapon
    {
        private void FixedUpdate()
        {
            CastForward();
        }
        
        //The server should just process this?
        private void CastForward()
        {
            Transform tr = transform;
            Vector3 position = tr.position;
            Vector3 forward = tr.forward;

            bool hitWall = Physics.Raycast(position, forward, out RaycastHit wallCheck, stats.MaxRange, StaticUtilities.GroundLayers);
            float dist = hitWall?wallCheck.distance:stats.MaxRange;

            int hitCount = Physics.SphereCastNonAlloc(position, stats.MaxRadius, forward, Hits, dist, StaticUtilities.EnemyLayer, PreviewCondition.Editor, 0.1f, Color.green, Color.red);
            
            for (int i = 0; i < hitCount; ++i)
            {
                Rigidbody n = Hits[i].rigidbody;
                if (n && n.TryGetComponent(out BallPlayer b) && b != _owner)
                {
                    //FIX this doesn't consider speed...
                    float dmg = _curDamage;
                    dmg *= _owner.Mass * _owner.GetBall.Speed;
                    print("Doing damage: " + dmg);
                    
                    DamageProperties damageProperties;
                    damageProperties.Damage = Mathf.Max(0, dmg);
                    damageProperties.Direction = forward * (dmg * stats.ForceMultiplier);
                    damageProperties.Attacker = OwnerClientId;
                    b.TakeDamage_ServerRpc(damageProperties);
                }
            }

            if (hitWall && !_isConnected)
            {
                enabled = false;
            }
        }

    }
}