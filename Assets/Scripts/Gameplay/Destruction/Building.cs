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

   
   public NetworkVariable<bool> IsDestroyed { get; private set; } = new NetworkVariable<bool>(false);

   public override void OnNetworkSpawn()
   {
      base.OnNetworkSpawn();

      IsDestroyed.Value = false;
      IsDestroyed.OnValueChanged += OnIsDestroyedChanged;
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
      if (IsServer)
      {
         IsDestroyed.Value = true;
      }
   }
   
   
}
