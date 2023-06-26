using UnityEngine;

public class Weapon : MonoBehaviour
{
    //[Header("Weapon Object")]
    [field: SerializeField] public bool HasAbility { get; private set; }

    private WeaponStats stats;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
