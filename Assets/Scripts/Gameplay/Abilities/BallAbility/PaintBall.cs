using Gameplay.Balls;
using Managers;
using Managers.Local;
using Managers.Network;
using UnityEngine;

namespace Gameplay.Abilities.BallAbility
{
    public class PaintBall : Ball
    {
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (!IsHost) return;
            NetworkGameManager.Instance.PlayParticleGlobally_ServerRpc("GlueExplosion", transform.position, transform.rotation);
            Vector3 pos = transform.GetChild(0).position;
            Collider[] cols=Physics.OverlapSphere(pos, 5, StaticUtilities.PlayerLayers);
            foreach (Collider c in cols)
            {
                Material createdMat = new Material(ParticleManager.GlueBallMat);
        
                //Kill me :(
                createdMat.SetFloat(StaticUtilities.ColorID, Random.Range(0,1f));
                createdMat.SetInt(StaticUtilities.RandomTexID, Random.Range(0,4));
                createdMat.SetVector(StaticUtilities.RandomOffsetID, new Vector4(Random.Range(-0.25f,0.25f),Random.Range(-0.25f,0.25f)));
             //   c.transform.parent.GetComponent<Ball>().ApplyEffect_ServerRpc(0);
            }
        }
    
    
    }
}
