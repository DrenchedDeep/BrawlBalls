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
            
            ToggleState_ServerRpc(false);
        }

        [ServerRpc]
        private void ToggleState_ServerRpc(bool state)
        {
            gameObject.SetActive(state);
        }
        
        
    

        protected virtual void OnTriggerEnter(Collider other)
        {
            Debug.LogWarning("Object collected");
            Award(other.transform.parent.GetComponent<BallPlayer>());
        }
    
    
    
    }
}
