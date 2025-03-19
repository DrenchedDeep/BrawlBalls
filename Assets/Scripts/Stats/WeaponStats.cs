using System.Text;
using Managers.Local;
using UnityEngine;

namespace Stats
{
    [CreateAssetMenu(fileName = "Weapon Stats", menuName = "Stats/WeaponStats", order = 3)]
    public class WeaponStats : BaseSelectorStats
    {
        [Header("Weapon")]
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float Mass { get; private set; }
        [field: SerializeField] public float ForceMultiplier { get; private set; }
        [field: SerializeField] public float MaxRange { get; private set; }
        [field: SerializeField] public float MaxRadius { get; private set; }
        [field: SerializeField] public AbilityStats Ability { get; private set; }
        [field: SerializeField] public LayerMask HitLayers { get; private set; }
        

        
        

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
    }
}
