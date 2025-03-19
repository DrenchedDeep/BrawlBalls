using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class ProjectileWeapon : NetworkBehaviour, IWeaponComponent
    {
        [SerializeField] private Transform firingPoint;

        private BallPlayer _ballPlayer;
        
        
        public void Init(BallPlayer owner)
        {
            _ballPlayer = owner;
        }

        /**
         * CLIENT FUNCTION: WERE DOING CLIENT AUTH FOR NOW SO CLIENTS WILL SPAWN THEIR OWN "REAL" PROJECTILE AND TELL THE SERVER WHAT THEY HIT
         */
        public void Fire(WeaponStats stats, out Vector3 velocity)
        {
            if (stats is not ProjectileWeaponStats projectileWeaponStats)
            {
                velocity = Vector3.zero;
                return;
            }
            
            Projectile projectile = Instantiate(projectileWeaponStats.ProjectilePrefab, firingPoint.position, firingPoint.rotation);
            projectile.Init(_ballPlayer, out velocity);
            
        }

        public void FireDummy(WeaponStats stats, Vector3 velocity)
        {
            ProjectileWeaponStats projectileWeaponStats = stats as ProjectileWeaponStats;

            if (!projectileWeaponStats)
            {
                return;
            }
            
            Projectile projectile = Instantiate(projectileWeaponStats.ProjectilePrefab, firingPoint.position, firingPoint.rotation);

            projectile.GetComponent<Rigidbody>().linearVelocity = velocity;
            projectile.enabled = false;
        }
    }
}