using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace Gameplay.Object_Scripts
{
    public class GlueObject : PlaceableObject
    {
        
        private Material m;

        private void Start()
        {
            //Material instance
            DecalProjector dp = GetComponent<DecalProjector>();
            m = new Material(dp.material);
        
            m.SetInt(StaticUtilities.RandomTexID, Random.Range(0,4));
            m.SetFloat(StaticUtilities.ColorID, Random.Range(0,1f));
            m.SetVector(StaticUtilities.RandomOffsetID, new Vector4(Random.Range(-0.25f,0.25f),Random.Range(-0.25f,0.25f)));
            dp.material = m;
        }

        protected override void OnHit(Ball hit)
        {
            //Verify that both the host and the person getting hit are affected by the same owner stats...
            //The Local player would store their stats in their script (Jagged array)
            //Then we have a server RPC called compare stats (Verify that they are the same)
            //However, if you're the host, then you must verify with a "buddy" just another player in the lobby.
            //if the change is allowed...
            //Apply duplicate material...
            print(hit);
            
            Material createdMat = new Material(ParticleManager.GlueBallMat);
        
            //Kill me :(
            createdMat.SetFloat(StaticUtilities.ColorID, m.GetFloat(StaticUtilities.ColorID));
            createdMat.SetInt(StaticUtilities.RandomTexID, m.GetInt(StaticUtilities.RandomTexID));
            createdMat.SetVector(StaticUtilities.RandomOffsetID, m.GetVector(StaticUtilities.RandomOffsetID));
            
            hit.ApplySlow(Owner, createdMat);
            Destroy(transform.parent.gameObject);
        }
    }
}
