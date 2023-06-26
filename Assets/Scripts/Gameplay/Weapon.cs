using UnityEngine;

public class Weapon : MonoBehaviour
{
    
    [SerializeField] private WeaponStats stats;

    //[Header("Weapon Object")]
    [field: SerializeField] public bool HasAbility { get; private set; }

    public float Mass => stats.Mass;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
