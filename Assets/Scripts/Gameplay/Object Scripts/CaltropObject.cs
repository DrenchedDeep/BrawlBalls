using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Object_Scripts
{
    public class CaltropObject : PlaceableObject
    {

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsOwnedByServer)
            {
                return;
            }
            Rigidbody rb = GetComponent<Rigidbody>();

            int i = Random.Range(0, 3);
      //      rb.AddForce(Quaternion.AngleAxis((i * 25) * ((i&1)==0?-1:1), axis) * new Vector3(0,150,0), ForceMode.Impulse);

            rb.AddForce(Vector3.up * 10, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 10, ForceMode.Impulse);
        }

        protected override void OnHit(BallPlayer hit)
        {
            //Again verify with upgrades and whatnot...
            Debug.Log("ball player hit spike: " + hit.name);
            hit.TakeDamage_ServerRpc(new DamageProperties(hit.GetBall.Speed*3, hit.GetBall.Velocity * -5f + Vector3.up*20, NetworkManager.Singleton.LocalClientId));
            
            //hit.TakeDamage(50, 15, owner.player);
         //   hit.TakeDamage_ClientRpc(hit.GetBall.Speed*3, hit.GetBall.Velocity * -5f + Vector3.up*20, NetworkManager.Singleton.LocalClientId);
        }
    }
}
