using UnityEngine;

namespace Stats
{
    [CreateAssetMenu(fileName = "Weapon Stats", menuName = "Stats/WeaponStats", order = 3)]
    public class WeaponStats : ScriptableObject
    {
        [field: SerializeField,TextArea] public string Description { get; private set; }
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float Mass { get; private set; }
        [field: SerializeField] public Vector2 Range { get; private set; }
        [field: SerializeField] public float PushMul { get; private set; }
        [field: SerializeField] public bool ForceBasedDamage { get; private set; }
    
        [field: SerializeField] public LayerMask HitLayers { get; private set; }
    
        [field: SerializeField] public AbilityStats Ability { get; private set; }
    
        [field: Header("Weapon")]
        [field: SerializeField] public float BaseDist { get; private set; }
    

    }
}
