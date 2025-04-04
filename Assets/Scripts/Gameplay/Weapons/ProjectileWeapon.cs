using Gameplay.Pools;
using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class ProjectileWeapon : NetworkBehaviour
    {
        [field: SerializeField] public Transform FiringPoint { get; private set; }
        [SerializeField] private ParticleSystem muzzleFlash;

        private BallPlayer _ballPlayer;
        
        
        public void Init(BallPlayer owner)
        {
            _ballPlayer = owner;
        }

        /**
         * CLIENT FUNCTION: WERE DOING CLIENT AUTH FOR NOW SO CLIENTS WILL SPAWN THEIR OWN "REAL" PROJECTILE AND TELL THE SERVER WHAT THEY HIT
         */
        public Vector3[] Fire(ProjectileWeaponStats stats, float inVelocity = 1)
        {

            Vector3[] vec = new Vector3[stats.ShotgunAmount];

            
            for (int i = 0; i < stats.ShotgunAmount; ++i)
            {
                Projectile projectile = ObjectPoolManager.Instance.GetObjectFromPool<Projectile>(stats.ProjectilePoolName, FiringPoint.position, FiringPoint.rotation);

                
                // Generate a random rotation within the spread angle
                Quaternion spreadRotation = Quaternion.Euler(
                    Random.Range(-stats.Spread, stats.Spread), // Random pitch (up/down)
                    Random.Range(-stats.Spread, stats.Spread), // Random yaw (left/right)
                    0 // No roll needed
                );

                // Apply spread to the forward direction
                vec[i] = spreadRotation * FiringPoint.forward * inVelocity;
                
                // Initialize projectile with spread
                projectile.Init(_ballPlayer, vec[i]);
            }

        
            PlayMuzzleFlash();
            
            return vec;
        }

        public void FireDummy(WeaponStats stats, Vector3 velocity)
        {
            if (stats is not  ProjectileWeaponStats ps)
            {
                return;
            }
            
            Debug.Log("Creating Dummy: " + ps.ProjectilePoolName);
            
            Projectile projectile = ObjectPoolManager.Instance.GetObjectFromPool<Projectile>(ps.ProjectilePoolName, FiringPoint.position, FiringPoint.rotation);
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