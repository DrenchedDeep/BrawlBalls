using UnityEngine;

namespace Stats
{
    public abstract class  BaseSelectorStats : ScriptableObject
    {
        [field: SerializeField,TextArea] public Sprite Icon { get; private set; }
        [field: SerializeField,TextArea] public ERarity Rarity { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }

        protected abstract string CreateCommonTraits();
        public string CommonTraits { get; private set; }
        
        private void OnEnable()
        {
            CommonTraits = CreateCommonTraits();
        }
    }

    public enum ERarity
    {
        Common,
        Rare,
        Epic,
        Ultra
    }
}
