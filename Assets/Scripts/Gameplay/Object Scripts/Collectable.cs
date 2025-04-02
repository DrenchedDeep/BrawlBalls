using Managers;
using Managers.Local;
using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Object_Scripts
{
    public class Collectable : NetworkBehaviour
    {
        [SerializeField] private ParticleManager.ECollectableType collectableType;
        // Add a reference to your particle system prefab (set it up in the Inspector)
        [SerializeField] private ParticleSystem particleEffectPrefab;

        protected void Award(BallPlayer getComponent)
        {
            // Award this collectable type to the owner.
            getComponent.GiveAward(collectableType);

            // Spawn the particle effect at the collectable's position
            ParticleSystem ps = Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            ps.Play();

            // Get the main module to access the duration
            ParticleSystem.MainModule mainModule = ps.main;
            // Destroy the particle system gameObject after its duration
            Destroy(ps.gameObject, mainModule.duration);

            // Disable the collectable object (networked)
            ToggleState_ServerRpc(false);
        }

        [ServerRpc]
        private void ToggleState_ServerRpc(bool state)
        {
            gameObject.SetActive(state);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            //Debug.LogWarning("Object collected");
            Award(other.transform.parent.GetComponent<BallPlayer>());
        }
    }
}