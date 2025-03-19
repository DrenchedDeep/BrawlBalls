using System.Text;
using Gameplay.Weapons;
using Managers.Local;
using Unity.Netcode;
using UnityEngine;

namespace Stats
{
    [CreateAssetMenu(fileName = "Projectile Weapon Stats", menuName = "Stats/ProjectileWeaponStats", order = 3)]
    public class ProjectileWeaponStats : WeaponStats
    {
        
        [field: SerializeField] public Projectile ProjectilePrefab { get; private set; }
        [field: SerializeField] public GameObject DummyProjectilePrefab { get; private set; }

    }
}
