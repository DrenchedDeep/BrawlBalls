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

        private NetworkVariable<bool> _isVisible = new NetworkVariable<bool>(true,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _isVisible.OnValueChanged += OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(bool old, bool current)
        {
            if(!current) collectionParticle.Play();
            visibility.SetActive(current);
        }

        protected void Award(BallPlayer getComponent)
        {
            Debug.Log("Award!");
            // Award this collectable type to the owner.
            getComponent.GiveAward(collectableType);

            

            // Disable the collectable object (networked)
            ToggleState_ServerRpc(false);
        }


        [ServerRpc(RequireOwnership = false)]
        private void ToggleState_ServerRpc(bool state)
        {
            _isVisible.Value = state;
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