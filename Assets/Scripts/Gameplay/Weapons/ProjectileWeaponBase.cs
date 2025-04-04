using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class ProjectileWeaponBase : BaseWeapon
    {
        [SerializeField] protected ProjectileWeapon[] projectileWeapons;
        private readonly CancellationTokenSource _rechargeCancelToken = new();
        [SerializeField] ParticleSystem chargingParticles;
        
        public bool IsRecharging { get; private set; }
        
        public override void Start()
        {
            base.Start();

            foreach (ProjectileWeapon wpn in projectileWeapons)
            {
                wpn.Init(Owner);
            }
        }

        protected override void Attack()
        {
            //Use IEnumerator in case we are destroyed.
            StartCoroutine(AttackReCharge());
        }


        private IEnumerator AttackReCharge()
        {
            IsRecharging = true;
            var x = ((ProjectileWeaponStats)stats);
            int n = x.BurstAmount;
            WaitForSeconds s = new WaitForSeconds(x.RefireTime);

            for (int i = 0; i < n; ++i)
            {
                Debug.Log("Waitin for refire: " + i);
                Fire();
                yield return s;
            }

            IsRecharging = false;
        }

        public override bool CanAttack()
        {
            return base.CanAttack() && !IsRecharging;
        }

        protected virtual void Fire()
        {
            ProjectileWeaponStats ps = (ProjectileWeaponStats)stats;
            for (int i = 0; i < projectileWeapons.Length; i++)
            {
                //fire locally
                Vector3[] vels =projectileWeapons[i].Fire(ps);
                
                //tell server to spawn projectiles for every other clients
                if (NetworkManager.Singleton)
                {
                    Debug.Log($"I am asking the server to fire {vels.Length} dummies");
                    Attack_ServerRpc(i, vels);
                }
            }
        }

        protected override void OnChargeStart()
        {
            chargingParticles.Play();
        }

        protected override void OnChargeStop()
        {
            chargingParticles.Stop();
        }

        //GPT generated
        private void OnDrawGizmosSelected()
        {
            for (int t = 0; t< projectileWeapons.Length; t++)
            {
                Vector3 origin = projectileWeapons[t].FiringPoint.position;
                Vector3 forward = projectileWeapons[t].FiringPoint.forward ;
                float spread = ((ProjectileWeaponStats)stats).Spread;

                // Directions with spread applied
                Vector3[] directions =
                {
                    Quaternion.Euler(0, 0, 0) * forward, // Forward
                    Quaternion.Euler(-spread, 0, 0) * forward, // Up
                    Quaternion.Euler(spread, 0, 0) * forward, // Down
                    Quaternion.Euler(0, spread, 0) * forward, // Right
                    Quaternion.Euler(0, -spread, 0) * forward // Left
                };

                Color[] colors = { Color.white, Color.green, Color.red, Color.blue, Color.yellow };

                for (int i = 0; i < directions.Length; i++)
                {
                    Gizmos.color = colors[i];
                    Gizmos.DrawRay(origin, directions[i] * 15);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        protected void Attack_ServerRpc(int index, Vector3[] projectileVelocities) => Attack_ClientRpc(index, projectileVelocities);

        [ClientRpc(RequireOwnership = false)]
        protected void Attack_ClientRpc(int weaponIndex, Vector3[] projectileVelocities)
        {
            if (!IsOwner)
            {
                foreach (Vector3 velocity in projectileVelocities)
                {
                    Debug.Log($"Firing Dummy with velocity {velocity}");
                    projectileWeapons[weaponIndex].FireDummy(stats, velocity);
                }
            }
        }
    }
}
