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
            ProjectileWeaponStats projectileWeaponStats = stats as ProjectileWeaponStats;

            if (!projectileWeaponStats)
            {
                velocity = Vector3.zero;
                return;
            }
            
            GameObject projectileGo = Instantiate(projectileWeaponStats.ProjectilePrefab, firingPoint.position, firingPoint.rotation);

            if (projectileGo.gameObject.TryGetComponent(out Projectile projectile))
            {
                projectile.Init(_ballPlayer, projectileWeaponStats, out velocity);
                return;
            }

            velocity = Vector3.zero;
        }

        public void FireDummy(WeaponStats stats, Vector3 velocity)
        {
            ProjectileWeaponStats projectileWeaponStats = stats as ProjectileWeaponStats;

            if (!projectileWeaponStats)
            {
                return;
            }
            
            GameObject projectileGo = Instantiate(projectileWeaponStats.DummyProjectilePrefab, firingPoint.position, firingPoint.rotation);

            if (projectileGo.gameObject.TryGetComponent(out DummyProjectile projectile))
            {
                projectile.Init(velocity, 3);
            }
        }
    }
}