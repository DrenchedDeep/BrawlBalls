using System;
using Gameplay;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability Stats", menuName = "Stats/AbilityStats", order = 2)]
public class AbilityStats : ScriptableObject
{
    [field: SerializeField,TextArea] public string Description { get; private set; }
    [field: SerializeField] public int Capacity { get; private set; }
    [field: SerializeField] public float Cooldown { get; private set; }
    [field: SerializeField] public Sprite Icon { get; set; }
    [SerializeField] private string abilityFileName;
    
    
    public Ability MyAbility { get; private set; }
    


    private void OnEnable()
    {
        MyAbility = Activator.CreateInstance(Type.GetType(abilityFileName) ?? throw new InvalidOperationException("Cannot convert to ability: " + abilityFileName)) as Ability;
    }
}
