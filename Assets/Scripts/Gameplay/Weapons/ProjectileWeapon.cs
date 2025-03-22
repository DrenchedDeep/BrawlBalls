using Gameplay.Pools;
using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class ProjectileWeapon : NetworkBehaviour
    {
        [SerializeField] private Transform firingPoint;
        [SerializeField] private ParticleSystem muzzleFlash;

        private BallPlayer _ballPlayer;
        
        
        public void Init(BallPlayer owner)
        {
            _ballPlayer = owner;
        }

        /**
         * CLIENT FUNCTION: WERE DOING CLIENT AUTH FOR NOW SO CLIENTS WILL SPAWN THEIR OWN "REAL" PROJECTILE AND TELL THE SERVER WHAT THEY HIT
         */
        public void Fire(WeaponStats stats, out Vector3 velocity, float inVelocity = 0)
        {
            if (stats is not ProjectileWeaponStats projectileWeaponStats)
            {
                velocity = Vector3.zero;
                return;
            }
            
            Projectile projectile = ObjectPoolManager.Instance.GetObjectFromPool<Projectile>(projectileWeaponStats.ProjectilePoolName,
                firingPoint.position, firingPoint.rotation);
            
            projectile.Init(_ballPlayer, out velocity);
            if (inVelocity > 0)
            {
                projectile.OverrideVelocity(firingPoint.forward * inVelocity);
                velocity = firingPoint.forward * inVelocity;
            }
        
            PlayMuzzleFlash();

        }

        public void FireDummy(WeaponStats stats, Vector3 velocity)
        {
            ProjectileWeaponStats projectileWeaponStats = stats as ProjectileWeaponStats;

            if (!projectileWeaponStats)
            {
                return;
            }
            
            Projectile projectile = ObjectPoolManager.Instance.GetObjectFromPool<Projectile>(projectileWeaponStats.ProjectilePoolName, firingPoint.position, firingPoint.rotation);
            projectile.Init();
            projectile.OverrideVelocity(velocity);
            PlayMuzzleFlash();
        }


        public void PlayMuzzleFlash()
        {
            if (muzzleFlash)
            {
                muzzleFlash.Play();
            }
        }
    }
}