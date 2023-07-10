using Unity.Netcode;
using UnityEngine;

public class Collectable : NetworkBehaviour
{
    

    [SerializeField] private ParticleManager.ECollectableType collectableType;
    
    
    protected void Award(Ball getComponent)
    {
        //Award this collectable type to the owner.
        
        //Play particle...
        
        //Destroy self...
        Destroy(gameObject);
    }
    

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Object collected");
        //Play effect...
        Award(other.transform.parent.GetComponent<Ball>());
       
    }
    
    
    
}
