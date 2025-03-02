using Managers;
using UnityEngine;

namespace Gameplay.Balls.BallAbility
{
    public class PaintNetworkBall : NetworkBall
    {
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (!IsHost) return;
            Level.Level.Instance.PlayParticleGlobally_ServerRpc("GlueExplosion", transform.position);
            Vector3 pos = transform.GetChild(0).position;
            Collider[] cols=Physics.OverlapSphere(pos, 5, StaticUtilities.PlayerLayers);
            foreach (Collider c in cols)
            {
                Material createdMat = new Material(ParticleManager.GlueBallMat);
        
                //Kill me :(
                createdMat.SetFloat(StaticUtilities.ColorID, Random.Range(0,1f));
                createdMat.SetInt(StaticUtilities.RandomTexID, Random.Range(0,4));
                createdMat.SetVector(StaticUtilities.RandomOffsetID, new Vector4(Random.Range(-0.25f,0.25f),Random.Range(-0.25f,0.25f)));
                c.transform.parent.GetComponent<NetworkBall>().ApplyEffectServerRpc(0);
            }
        }
    
    
    }
}
