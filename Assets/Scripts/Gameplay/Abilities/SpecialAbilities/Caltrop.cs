using Managers.Local;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Caltrop : Ability
    {
        public override bool CanUseAbility(BallPlayer owner) => true;

        public override void ExecuteAbility(BallPlayer owner)
        {
            SpawnCaltrops_ServerRpc("Caltrop", owner.transform.position + Vector3.one * 6,  Quaternion.identity, owner.GetBall.Velocity.normalized, 150);
        }

        public override void CancelAbility(BallPlayer owner)
        {
            throw new System.NotImplementedException();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnCaltrops_ServerRpc(string objectName, Vector3 location, Quaternion rotation, Vector3 axis, float force, ServerRpcParams @params = default)
        {
            Debug.Log("Caltrops Spawning -- ADD SERVER SIDE VALIDATION");
            
            for (int i = 0; i < 3; ++i)
            {
                NetworkObject ngo = Object.Instantiate(ResourceManager.SummonableObjects[objectName], location, rotation);
                ngo.GetComponent<Rigidbody>().AddForce(Quaternion.AngleAxis((i * 25) * ((i&1)==0?-1:1), axis) * new Vector3(0,force,0), ForceMode.Impulse);
                ngo.SpawnWithOwnership(@params.Receive.SenderClientId);
                
                Debug.DrawRay(location, Quaternion.AngleAxis((i * 25) * ((i&1)==0?-1:1), axis) * new Vector3(0,force,0), Color.yellow, 4);
            }
        }
    }
}
