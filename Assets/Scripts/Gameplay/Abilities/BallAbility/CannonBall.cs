using Gameplay.Balls;
using Managers;
using Managers.Local;
using Managers.Network;
using UnityEngine;

//Should probably commit to how we want abilities to work... either like in AbilityStats or like this.


namespace Gameplay.Abilities.BallAbility
{
    public class CannonBall : Ball
    {
        private const float MaxDist = 10;

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (!IsServer) return; // Only the server should be able to handle this logic.
        
            NetworkGameManager.Instance.PlayParticleGlobally_ServerRpc("Explosion", transform.position, transform.rotation);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.RocketExplosion, transform.position);
        
            Vector3 pos = transform.position;
            Collider[] cols = Physics.OverlapSphere(pos, 10, StaticUtilities.PlayerLayers);
            foreach (Collider c in cols)
            {
                Vector3 ePos = c.ClosestPoint(pos);
                Vector3 dir = ePos - pos;
                float damage = ParticleManager.EvalauteExplosiveDistance(dir.magnitude / MaxDist)*200;
                
                DamageProperties damageProperties;
                damageProperties.Damage = damage;
                damageProperties.Direction = damage * dir;
                damageProperties.Attacker = OwnerClientId;
                damageProperties.ChildID = BallPlayer.ChildID.Value;


                c.attachedRigidbody.GetComponent<BallPlayer>().TakeDamage_ServerRpc(damageProperties);
            }
        }
    }
}
