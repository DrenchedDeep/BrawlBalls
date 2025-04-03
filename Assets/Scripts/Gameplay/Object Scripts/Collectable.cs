using Managers.Local;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Object_Scripts
{
    public class Collectable : NetworkBehaviour
    {
        [SerializeField] private ParticleManager.ECollectableType collectableType;
        // Add a reference to your particle system prefab (set it up in the Inspector)
        [SerializeField] protected ParticleSystem collectionParticle;
        [SerializeField] private GameObject visibility;

        protected void Award(BallPlayer getComponent)
        {
            // Award this collectable type to the owner.
            getComponent.GiveAward(collectableType);

            

            // Disable the collectable object (networked)
            ToggleState_ServerRpc(false);
        }


        [ServerRpc(RequireOwnership = false)]
        private void ToggleState_ServerRpc(bool state)
        {
            if(!state) collectionParticle.Play();
            visibility.SetActive(state);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            //Debug.LogWarning("Object collected");
            Rigidbody rb = other.attachedRigidbody;
            if (rb && rb.TryGetComponent(out BallPlayer ballPlayer))
                OnCollection(rb, ballPlayer);
        }

        protected virtual void OnCollection(Rigidbody rb, BallPlayer ballPlayer)
        {
            Award(ballPlayer);
        }
    }
}