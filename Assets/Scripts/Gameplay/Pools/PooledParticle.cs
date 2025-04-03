using System.Threading;
using Gameplay.Pools;
using UnityEngine;

public class PooledParticle : PooledObject
{ 
   [SerializeField] private ParticleSystem[] particles;
  
  
  
   public override void OnTakenFromPool()
   {
      foreach (ParticleSystem particle in particles)
      {
        particle.Play();
      }
      PoolCancellation = new CancellationTokenSource();
      _ = ReturnToPoolTask(PoolCancellation);
   }
   
   
}
