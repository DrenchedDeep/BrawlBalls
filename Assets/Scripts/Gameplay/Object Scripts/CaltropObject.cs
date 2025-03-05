using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Object_Scripts
{
    public class CaltropObject : PlaceableObject
    {

        private void Start()
        {
            if(!IsOwnedByServer) return;
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * 10, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 10, ForceMode.Impulse);
        }

        protected override void OnHit(BallPlayer hit)
        {
            //Again verify with upgrades and whatnot...
            
            hit.TakeDamage_ServerRpc(new DamageProperties(hit.GetBall.Speed*3, hit.GetBall.Velocity * -5f + Vector3.up*20, NetworkManager.Singleton.LocalClientId));
            
            //hit.TakeDamage(50, 15, owner.player);
         //   hit.TakeDamage_ClientRpc(hit.GetBall.Speed*3, hit.GetBall.Velocity * -5f + Vector3.up*20, NetworkManager.Singleton.LocalClientId);
        }
    }
}
