using Gameplay.Object_Scripts;
using UnityEngine;

public class DiamondSpawnPoint : MonoBehaviour
{
    public bool CanSpawnDiamond => _diamond == null;


    private Collectable _diamond;

    public void Init(Collectable diamond)
    {
        _diamond = diamond;
    }
    
    
    
}
