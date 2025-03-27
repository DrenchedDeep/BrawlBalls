using Managers;
using Managers.Local;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace Gameplay.Object_Scripts
{
    public class GlueObject : PlaceableObject
    {
        
        private Material _material;

        private void Start()
        {
            if (!IsOwnedByServer) return;
            //Material instance
          
            //Pass through client RPC so it's the same color for everyone. Not super necessary, but an appreciated QOL change.
            SetMaterialClientRpc(Random.Range(0,4), Random.Range(0,1f), new Vector2(Random.Range(-0.25f,0.25f),Random.Range(-0.25f,0.25f)));
           
        }

        
        
        [ClientRpc]
        private void SetMaterialClientRpc(int a, float b, Vector2 c)
        {
            DecalProjector dp = GetComponent<DecalProjector>();
            _material = new Material(dp.material);
            
            _material.SetInt(StaticUtilities.RandomTexID, a);
            _material.SetFloat(StaticUtilities.ColorID, b);
            _material.SetVector(StaticUtilities.RandomOffsetID, c);
            
            dp.material = _material;
        }



        protected override void OnHit(BallPlayer hit)
        {
            //Verify that both the host and the person getting hit are affected by the same owner stats...
            //The Local player would store their stats in their script (Jagged array)
            //Then we have a server RPC called compare stats (Verify that they are the same)
            //However, if you're the host, then you must verify with a "buddy" just another player in the lobby.
            //if the change is allowed...
            //Apply duplicate material...
            
            //We know at this point, only the host of the game is actually tracking this, as the component would have already disabled itself at this point.
            
            print(hit);
            
            Material createdMat = new Material(ParticleManager.GlueBallMat);
        
            //Kill me :(
            createdMat.SetFloat(StaticUtilities.ColorID, _material.GetFloat(StaticUtilities.ColorID));
            createdMat.SetInt(StaticUtilities.RandomTexID, _material.GetInt(StaticUtilities.RandomTexID));
            createdMat.SetVector(StaticUtilities.RandomOffsetID, _material.GetVector(StaticUtilities.RandomOffsetID));
            
         //   hit.GetBall.ApplyEffect_ServerRpc(0);

            NetworkObject.Despawn();

            //Destroy(transform.parent.gameObject);
        }
    }
}
