using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkObject))]
public class Building : NetworkBehaviour
{
   [SerializeField] private ParticleSystem particles;
   [SerializeField] private GameObject defaultBuilding;
   [SerializeField] private GameObject destroyedBuilding;
   [SerializeField] private GameObject[] otherObjectsToDestroy;

   
   private NetworkVariable<bool> _isDestroyed = new NetworkVariable<bool>(false);

   public override void OnNetworkSpawn()
   {
      base.OnNetworkSpawn();

      _isDestroyed.Value = false;
      _isDestroyed.OnValueChanged += OnIsDestroyedChanged;
   }

   private void OnIsDestroyedChanged(bool old, bool current)
   {
      if (current)
      {
         defaultBuilding.SetActive(false);
         destroyedBuilding.SetActive(true);
         particles.Play();

         foreach (GameObject go in otherObjectsToDestroy)
         {
            go.SetActive(false);
         }
      }
   }
   
   //server
   public void DestroyBuilding()
   {
      _isDestroyed.Value = true;
   }
   
   
}
