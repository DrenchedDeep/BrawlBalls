using System;
using System.Collections.Generic;
using System.Text;
using Gameplay;
using Gameplay.Abilities.SpecialAbilities;
using Gameplay.Abilities.WeaponAbilities;
using UnityEngine;

namespace Stats
{
    [CreateAssetMenu(fileName = "Ability Stats", menuName = "Stats/AbilityStats", order = 2)]
    public class AbilityStats : BaseSelectorStats
    {
        [field: SerializeField] public int Capacity { get; private set; }
        [field: SerializeField] public float Cooldown { get; private set; }
        [SerializeField] private string abilityFileName;


        private static readonly Dictionary<string, Func<Ability>> AbilityDataSet = new()
        {
            { "Caltrop", () => new Caltrop() },
            { "Glue", () => new Glue() },
            { "Jump", () => new Jump() },
            { "Portal", () => new Portal() },
            { "Protect", () => new Protect() },

            { "Abductor", () => new Abductor() },
            { "Laserbeam", () => new Laserbeam() },
            { "Spike", () => new Spike() },
        };

        private Ability _ability;

        public Ability Ability
        {
            get
            {
                if (_ability == null && AbilityDataSet.TryGetValue(abilityFileName, out Func<Ability> factory))
                {
                    _ability = factory();
                }
                else if (_ability == null)
                {
                    Debug.LogError($"Ability '{abilityFileName}' not found in factory. Check AbilityStats.cs and or your spelling.");
                }

                return _ability;
            }
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if(Ability == null) Debug.LogError($"Ability '{abilityFileName}' not found in factory. Check AbilityStats.cs and or your spelling.");
        }
        #endif
        protected override string CreateCommonTraits()
        {
            StringBuilder st = new();
            st.AppendLine("<sprite=5>" + (Capacity > 0? Capacity.ToString() : "<sprite=6>"));
            st.AppendLine("<sprite=7>" + (Cooldown > 0? Cooldown.ToString("F1") : "<sprite=6>"));
            
            return st.ToString();
        }
    }
}