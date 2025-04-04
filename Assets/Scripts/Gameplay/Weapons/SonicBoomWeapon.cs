using Gameplay;
using Gameplay.Weapons;
using RotaryHeart.Lib.PhysicsExtension;
using Unity.Netcode;
using UnityEngine;
using Physics = UnityEngine.Physics;

public class SonicBoomWeapon : BaseWeapon
{
    [SerializeField] private Transform fireTransform;
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private float detectRadius = 0.5f;
    [SerializeField] private float castDistance = 5f;
    [SerializeField] private float speedBoost = 2000;
    
    private NetworkVariable<int> onFire = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        onFire.OnValueChanged += OnFire;
    }
    

    private void OnFire(int old, int current)
    {
        fireParticles.Play();
    }

    protected override void Attack()
    {
        onFire.Value++;

        Vector3 origin = fireTransform.position;
        Vector3 direction = fireTransform.forward;
        RaycastHit[] hits = new RaycastHit[10];
        int hitCount = Physics.SphereCastNonAlloc(origin, detectRadius, direction, hits, castDistance, stats.HitLayers);
        
        if (hitCount > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.TryGetComponent(out BallPlayer player))
                {
                    Debug.Log("sonic boom hit player: " + player.OwnerClientId);
                    if (CanDamage(player))
                    {
                        Debug.Log("is not my owner");

                        player.TakeDamage_ServerRpc(new DamageProperties(0, direction * speedBoost, Owner.OwnerClientId,
                            Owner.ChildID.Value));
                        //    player.GetBall.AddImpulse_ServerRpc(direction * speedBoost);
                    }
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
