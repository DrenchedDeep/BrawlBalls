using Gameplay;
using Gameplay.Weapons;
using RotaryHeart.Lib.PhysicsExtension;
using Unity.Netcode;
using UnityEngine;
using Physics = UnityEngine.Physics;

public class SonicBoomWeapon : BaseWeapon
{
    [SerializeField] private Transform fireTransform;
    [SerializeField] private float detectRadius = 0.5f;
    [SerializeField] private float castDistance = 5f;
    [SerializeField] private float speedBoost = 2000;
    
    protected override void Attack()
    {
        Vector3 origin = fireTransform.position;
        Vector3 direction = fireTransform.forward;
        RaycastHit[] hits = new RaycastHit[10];
        int hitCount = Physics.SphereCastNonAlloc(origin, detectRadius, direction, hits, castDistance, stats.HitLayers);
        
#if UNITY_EDITOR
        DebugExtensions.DebugSphereCast(origin, direction, castDistance, Color.green, detectRadius, 5f,
            CastDrawType.Complete, PreviewCondition.Both, true);
#endif
        
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.TryGetComponent(out BallPlayer player))
            {
                Debug.Log("sonic boom hit player: " + player.OwnerClientId);
                if (CanDamage(player))
                {
                    Debug.Log("is not my owner");

                    player.TakeDamage_ServerRpc(new DamageProperties(0, direction * speedBoost, Owner.OwnerClientId, Owner.ChildID.Value));
                //    player.GetBall.AddImpulse_ServerRpc(direction * speedBoost);
                }
            }
        }
    }
    
    private bool CanDamage(BallPlayer b)
    {
        if (b.OwnerClientId == Owner.OwnerClientId)
        {
            return b.ChildID.Value != Owner.ChildID.Value;
        }
            
        return true;
    }
    
}
