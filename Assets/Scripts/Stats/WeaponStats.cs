using System.Text;
using UnityEngine;

namespace Stats
{
    [CreateAssetMenu(fileName = "Weapon Stats", menuName = "Stats/WeaponStats", order = 3)]
    public class WeaponStats : BaseSelectorStats
    {
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float Mass { get; private set; }
        [field: SerializeField] public Vector2 Range { get; private set; }
        [field: SerializeField] public float PushMul { get; private set; }
        [field: SerializeField] public bool ForceBasedDamage { get; private set; }
        [field: SerializeField] public bool Force { get; private set; }
    
        [field: SerializeField] public LayerMask HitLayers { get; private set; }
    
        [field: SerializeField] public AbilityStats Ability { get; private set; }
    
        [field: Header("Weapon")]
        [field: SerializeField] public float BaseDist { get; private set; }


        protected override string CreateCommonTraits()
        {
            StringBuilder st = new();
            st.AppendLine($"<sprite=8>{Damage}");
            st.AppendLine($"<sprite=8>{Mass}");
            st.AppendLine($"<sprite=9>{Range}");
            st.AppendLine($"<sprite=10>{Force}");
            
            return st.ToString();
        }
    }
}
