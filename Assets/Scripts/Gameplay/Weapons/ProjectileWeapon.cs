using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class ProjectileWeapon : MonoBehaviour, IWeaponComponent
    {
        [SerializeField] private Transform firingPoint;

        private BallPlayer _ballPlayer;
        
        public void Init(BallPlayer owner)
        {
            _ballPlayer = owner;
        }

        public void Fire(WeaponStats stats)
        {
            ProjectileWeaponStats projectileWeaponStats = stats as ProjectileWeaponStats;

            if (!projectileWeaponStats)
            {
                return;
            }
            
            NetworkObject networkObject = Instantiate(projectileWeaponStats.ProjectilePrefab, firingPoint.position, firingPoint.rotation);

            if (networkObject.gameObject.TryGetComponent(out Projectile projectile))
            {
                projectile.Init(_ballPlayer, projectileWeaponStats);
            }
        }
    }
}