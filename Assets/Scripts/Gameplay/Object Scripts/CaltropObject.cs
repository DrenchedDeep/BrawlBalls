using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Object_Scripts
{
    public class CaltropObject : PlaceableObject
    {
        
        private Rigidbody _rigidbody;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsOwnedByServer)
            {
                return;
            }
            _rigidbody = GetComponent<Rigidbody>();
            
            _rigidbody.AddForce(Vector3.up * 10, ForceMode.Impulse);
            _rigidbody.AddTorque(Random.insideUnitSphere * 10, ForceMode.Impulse);
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
            
           // _rigidbody.isKinematic = true;
        }

        protected override void OnHit(BallPlayer hit)
        {
            //Again verify with upgrades and whatnot...
            Debug.Log("ball player hit spike: " + hit.name);
            hit.TakeDamage_ServerRpc(new DamageProperties(hit.GetBall.Speed*3, hit.GetBall.Velocity * -5f + Vector3.up*20, NetworkManager.Singleton.LocalClientId, -1));
            
            //hit.TakeDamage(50, 15, owner.player);
         //   hit.TakeDamage_ClientRpc(hit.GetBall.Speed*3, hit.GetBall.Velocity * -5f + Vector3.up*20, NetworkManager.Singleton.LocalClientId);
        }
    }
}
