using UnityEngine;

namespace Stats
{
    [CreateAssetMenu(fileName = "ShopItemStats", menuName = "Scriptable Objects/ShopItemStats")]
    public class ShopItemStats : ScriptableObject
    {
        [field: SerializeField] public BaseSelectorStats Stats { get; private set; }
        [field: SerializeField] public GameObject Prefab { get; private set; }

        [field: SerializeField] public bool IsLocked { get; private set; }

    }
}
