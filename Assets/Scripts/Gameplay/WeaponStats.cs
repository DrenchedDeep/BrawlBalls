using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Stats", menuName = "Stats/WeaponStats", order = 3)]
public class WeaponStats : ScriptableObject
{
    [field: SerializeField,TextArea] public string Description { get; private set; }
    [field: SerializeField] public float Damage { get; private set; }
    [field: SerializeField] public float Mass { get; private set; }
    [field: SerializeField] public float Range { get; private set; }

}
