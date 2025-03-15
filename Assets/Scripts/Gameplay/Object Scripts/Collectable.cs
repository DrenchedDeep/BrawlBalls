using Managers;
using Managers.Local;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Object_Scripts
{
    public class Collectable : NetworkBehaviour
    {
    

        [SerializeField] private ParticleManager.ECollectableType collectableType;
    
    
        protected void Award(BallPlayer getComponent)
        {
            //Award this collectable type to the owner.
            getComponent.GiveAward(collectableType);
            //Play particle...
        
            //Destroy self...
            Destroy(gameObject);
        }
    

        protected virtual void OnTriggerEnter(Collider other)
        {
            Debug.LogWarning("Object collected");
            //Play effect...
            Award(other.transform.parent.GetComponent<BallPlayer>());
       
        }
    
    
    
    }
}
