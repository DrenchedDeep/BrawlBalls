using Managers;
using UnityEngine;

namespace Gameplay.Balls.BallAbility
{
    public class CannonNetworkBall : NetworkBall
    {
        private const float MaxDist = 10;

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (!IsHost) return; // Only the host should be able to handle this logic.
        
            Level.Level.Instance.PlayParticleGlobally_ServerRpc("Explosion", transform.position);
        
            Vector3 pos = transform.GetChild(0).position;
            Collider[] cols = Physics.OverlapSphere(pos, 5, StaticUtilities.PlayerLayers);
            foreach (Collider c in cols)
            {
                Vector3 ePos = c.ClosestPoint(pos);
                Vector3 dir = ePos - pos;
                float damage = ParticleManager.EvalauteExplosiveDistance(dir.magnitude / MaxDist)*200;
                c.transform.parent.GetComponent<NetworkBall>().TakeDamageClientRpc(damage, damage * dir, OwnerClientId);
            }
        }
    }
}
