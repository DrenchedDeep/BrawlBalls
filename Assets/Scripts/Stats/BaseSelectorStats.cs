using UnityEngine;

namespace Stats
{
    public abstract class  BaseSelectorStats : ScriptableObject
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public ERarity Rarity { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }

        protected abstract string CreateCommonTraits();
        public string CommonTraits { get; private set; }
        
        private void OnEnable()
        {
            CommonTraits = CreateCommonTraits();
        }
        public enum ERarity
        {
            Common,
            Rare,
            Epic,
            Ultra
        }

    }

}
