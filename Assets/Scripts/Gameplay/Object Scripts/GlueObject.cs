using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace Gameplay.Object_Scripts
{
    public class GlueObject : PlaceableObject
    {
        private static readonly int RandomTexID = Shader.PropertyToID("_Tex");
        private static readonly int RandomColorID = Shader.PropertyToID("_Color");
        private static readonly int RandomOffsetID = Shader.PropertyToID("_Offset");
        private Material m;

        private void Start()
        {
            //Material instance
            DecalProjector dp = GetComponent<DecalProjector>();
            m = new Material(dp.material);
        
            m.SetInt(RandomTexID, Random.Range(0,4));
            m.SetFloat(RandomColorID, Random.Range(0,1f));
            m.SetVector(RandomOffsetID, new Vector4(Random.Range(-0.25f,0.25f),Random.Range(-0.25f,0.25f)));
            dp.material = m;

            transform.eulerAngles = new Vector3(90, Random.Range(0, 360), 0);

        }

        protected override void OnHit(Ball hit)
        {
            //Verify that both the host and the person getting hit are affected by the same owner stats...
            //The Local player would store their stats in their script (Jagged array)
            //Then we have a server RPC called compare stats (Verify that they are the same)
            //However, if you're the host, then you must verify with a "buddy" just another player in the lobby.
            //if the change is allowed...
            //Apply duplicate material...
            hit.ApplySlow(owner, m);
            Destroy(transform.parent.gameObject);
        }
    }
}
