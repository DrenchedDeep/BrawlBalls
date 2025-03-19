using UnityEngine;

namespace Gameplay.Weapons
{
    public class ProjectileWeapon : MonoBehaviour
    {
        [SerializeField] private Transform firingPoint;
       // [SerializeField] private Networkob
        
        //Weapons are enabled / disabled when the user begins pressing. This is controlled by the ability
        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }
    }
}