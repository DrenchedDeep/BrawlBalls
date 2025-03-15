using System.Collections;
using Cysharp.Threading.Tasks;
using Gameplay.Weapons;
using Managers.Local;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Abilities.WeaponAbilities
{
    public class Spike : Ability
    {
        private bool _hasSpike = true;
        public override bool CanUseAbility(BallPlayer owner) => owner.GetBall.Speed > 3 && _hasSpike;

        public override void ExecuteAbility(BallPlayer owner)
        {
            Debug.Log("Shot the spike!");
            //Un parent self
            _hasSpike = false;
            //owner.GetBaseWeapon.Disconnect(owner.GetBall.Speed);
        }

        private async UniTask ReturnToPlayer()
        {
            // Hide the players spike weapon (Disable collisions)
            // Spawn a spike projectile
            // Add force to the spike weapon
            await UniTask.Delay(15000);
            // "Respawn" the player's spike
            _hasSpike = true;
        }

/*
        public static IEnumerator Move(BaseWeapon baseWeapon, float speed)
        {
            Transform ownerTrans = baseWeapon.transform;
            float duration = 5;
            while (duration > 0)
            {
                duration -= Time.deltaTime;
                ownerTrans.position += speed * Time.deltaTime * ownerTrans.forward;
                if (Physics.Raycast(ownerTrans.position, ownerTrans.forward, out RaycastHit hit, baseWeapon.Stats.MaxRange , StaticUtilities.GroundLayers)) // 1==Default
                {
                    ownerTrans.position = hit.point - ownerTrans.forward * baseWeapon.Stats.MaxRange;
                    baseWeapon.enabled = false;
                    yield break;
                }
                yield return null;
            }
            baseWeapon.NetworkObject.Despawn();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void DisconnectServerRpc(float speed)
        {
            NetworkObject.TryRemoveParent();
            DisconnectClientRpc();
            owner.StartCoroutine(Spike.Move(this , speed * 5)); // Owner is just the object running the coroutine
        }

        [ClientRpc]
        private void DisconnectClientRpc()
        {
            gameObject.layer = 0;
            GetComponent<BoxCollider>().enabled = true;
            if (!IsHost)
            {
                enabled = false;
            }
        }

        public void Disconnect(float speed)
        {
            DisconnectServerRpc(speed);
        }
        */
    }
}
