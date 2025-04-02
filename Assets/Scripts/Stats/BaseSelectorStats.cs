#if UNITY_EDITOR
using System;
using System.Text;
#endif
using UnityEngine;

namespace Stats
{
    public enum ERarity
    {
        Common,
        Rare,
        Epic,
        Ultra
    }
    public abstract class BaseSelectorStats : ScriptableObject
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public ERarity Rarity { get; private set; }
        [field: SerializeField] public string Comment { get; private set; }

        [field: SerializeField, HideInInspector] private string description;
        [field: SerializeField, HideInInspector] private string statsContext;
        
        public string Description => description;
        public string StatsContext => statsContext;


#if UNITY_EDITOR
        [SerializeField] protected DescriptionTag[] tags;
        
        protected abstract string CreateCommonTraits();

        protected virtual void OnValidate()
        {
            StringBuilder sb = new StringBuilder();

            if (tags == null)
            {
                
                Debug.Log(name + " has no tags", this);
                return;
            }
            Array.Sort(tags, (a, b) => b.traitType.CompareTo(a.traitType)); // Sort by trait type in descending order


            for (int i = 0; i < tags.Length; ++i)
                sb.Append(tags[i].GetDisplayText());
            description = sb.ToString();

            statsContext = CreateCommonTraits();
        }
#endif

    }

#if UNITY_EDITOR
        public enum ETraitType
        {
            VeryNegative = -2,
            Negative = -1,
            Neutral = 0,
            Positive = 1,
            VeryPositive =2,
        }
        [Serializable]
        public struct DescriptionTag
        {
            public ETraitType traitType;
            [SerializeField] private string information;

            public string GetDisplayText()
            {
                int traitValue = (int)traitType;

                StringBuilder sb = new();

                switch (traitType)
                {
                    case ETraitType.VeryNegative:
                        sb.Append("<color=#d14c45>"); // Light Red
                        break;
                    case ETraitType.Negative:
                        sb.Append("<color=#FFA07A>"); // Light Salmon
                        break;
                    case ETraitType.Positive:
                        sb.Append("<color=#98FB98>"); // Pale Green
                        break;
                    case ETraitType.VeryPositive:
                        sb.Append("<color=#47a647>"); // Light Green
                        break;
                    default:
                        sb.Append("<color=#FFFFFF>"); // White
                        break;
                }

                switch (traitValue)
                {
                  case 0:
                      sb.Append("• ");
                      break;
                  case < 0:
                      sb.Append("- ");
                      break;
                  case > 0:
                      sb.Append("+ ");
                      break;
                }

                sb.Append(information);
                sb.Append("</color>\n");
                
                return sb.ToString();
            }
        }        
        

#endif

}
