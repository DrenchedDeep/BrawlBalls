using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability Stats", menuName = "Stats/AbilityStats", order = 2)]
public class AbilityStats : ScriptableObject
{
    [field: SerializeField,TextArea] public string Description { get; private set; }
    [field: SerializeField] public int Capacity { get; private set; }
    [SerializeField] private float cooldown;
    [SerializeField] private string abilityFileName;
    
    
    public Ability MyAbility { get; private set; }
    
    public WaitForSeconds Cooldown;


    private void OnEnable()
    {
        Cooldown = new WaitForSeconds(cooldown);
        MyAbility = Activator.CreateInstance(Type.GetType(abilityFileName) ?? throw new InvalidOperationException()) as Ability;
        if (MyAbility != null)
        {
            Debug.Log(MyAbility.GetType());
           
        }
        Debug.Log(name);
    }
}
