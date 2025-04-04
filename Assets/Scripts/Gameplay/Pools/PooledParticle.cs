using System.Threading;
using Gameplay.Pools;
using UnityEngine;

public class PooledParticle : PooledObject
{ 
   [SerializeField] private ParticleSystem particle;
  
  
  
   public override void OnTakenFromPool()
   {
      particle.Play();
      PoolCancellation = new CancellationTokenSource();
      _ = ReturnToPoolTask(PoolCancellation);
   }
   
   
}
