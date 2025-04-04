using System.Text;
using Managers.Local;
using Unity.Netcode;
using UnityEngine;
namespace Stats
{
    [CreateAssetMenu(fileName = "Projectile Stats", menuName = "Stats/Projectile Stats", order = 3)]
    public class ProjectileStats : WeaponStats
    {
        public enum ProjectileDamageType
        {
            Single,
            Radial
        }

        [field: SerializeField] public float InitialVelocity { get; private set; }
        [field: SerializeField] public float GravMult { get; private set; } = 1;

        [field: SerializeField] public bool  BallVelocityAffectsProjectileVelocity { get; private set; }
        [field: SerializeField] public float MaxLifetime { get; private set; }
        [field: SerializeField] public bool  IsHomingProjectile { get; private set; }
        [field: SerializeField] public bool  IsAffectedByGravity { get; private set; }
        [field: SerializeField] public bool  RotateTowardsVelocity { get; private set; }
        [field: SerializeField] public bool  DoDamageAfterCollision { get; private set; }

        [field: SerializeField] public ProjectileDamageType DamageType { get; private set; }
        [field: SerializeField] public float EffectDisableTime { get; private set; }
        [field: SerializeField] public string HitVfxPoolName { get; private set; }


#if UNITY_EDITOR
        protected override string CreateCommonTraits()
        {
            StringBuilder st = new();
            st.AppendLine($"<sprite=8>{Damage}");
            st.AppendLine($"<sprite=8>{Mass}");
            st.AppendLine($"<sprite=9>{MaxRange}");
            st.AppendLine($"<sprite=10>{ForceMultiplier}");
            if (((int)HitLayers & StaticUtilities.LocalBallLayer) != 0)
            {
                st.AppendLine($"<sprite=11>Hits Team");
            }

            return st.ToString();
        }
        #endif
    }
}
