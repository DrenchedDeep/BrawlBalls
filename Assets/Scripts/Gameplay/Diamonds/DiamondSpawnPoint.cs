using Gameplay.Object_Scripts;
using UnityEngine;

public class DiamondSpawnPoint : MonoBehaviour
{
    [SerializeField] private Building building;
    public bool CanSpawnDiamond => (_diamond == null && (building ? !building.IsDestroyed.Value : true));


    private Collectable _diamond;

    public void Init(Collectable diamond)
    {
        _diamond = diamond;
    }
    
    
    
}
