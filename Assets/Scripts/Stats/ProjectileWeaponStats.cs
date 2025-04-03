using UnityEngine;

namespace Stats
{
    [CreateAssetMenu(fileName = "Projectile Weapon Stats", menuName = "Stats/ProjectileWeaponStats", order = 3)]
    public class ProjectileWeaponStats : WeaponStats
    {
        [field: SerializeField] public string ProjectilePoolName { get; private set; }
        
        //how long until the weapon must recharge.. kinda like its "overheating"
        [field: SerializeField, Min(1),Tooltip("Number of bursts to do")] public int BurstAmount { get; private set; } = 1;
        [field: SerializeField, Min(1),Tooltip("Number of Projectiles fired")] public int ShotgunAmount { get; private set; } = 1;
        [field: SerializeField,Tooltip("How long does it take between burst shots")] public float RefireTime { get; private set; }
        [field: SerializeField,Range(0,90),Tooltip("How Inaccurate is the weapons shots in degrees. 0 is perfectly straight")] public float Spread { get; private set; }
    }
}
