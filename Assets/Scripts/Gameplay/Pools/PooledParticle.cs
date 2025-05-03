using System.Threading;
using Gameplay.Pools;
using UnityEngine;

public class PooledParticle : PooledObject
{ 
   [SerializeField] private ParticleSystem particle;
  
  
  
   public override void OnTakenFromPool()
   {
      particle.Play();
      // NO FUCKING WONDER WHY AUDIO JUST PLAYS DAWG OML, GONNA CRY MYSELF TO SLEEP.
      PoolCancellation = new CancellationTokenSource();
      _ = ReturnToPoolTask(PoolCancellation);
   }
   
   
}
