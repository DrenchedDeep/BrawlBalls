using UnityEngine;

[CreateAssetMenu(fileName = "Ball Stats", menuName = "Stats/BallStats", order = 1)]
public class BallStats : ScriptableObject
{
    [field: SerializeField,TextArea] public string Description { get; private set; }
    [field: SerializeField] public float MaxHealth { get; private set; }
    [field: SerializeField] public float MaxSpeed { get; private set; }
    [field: SerializeField] public float TurningSpeed { get; private set; }
    [field: SerializeField] public float Acceleration { get; private set; }
    [field: SerializeField] public float Mass { get; private set; }
    
}
