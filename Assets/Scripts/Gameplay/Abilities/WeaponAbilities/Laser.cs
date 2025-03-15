using UnityEngine;

namespace Gameplay.Abilities.WeaponAbilities
{
    public class Laser : Ability
    {
        public override bool CanUseAbility(BallPlayer owner) => true;

        public override void ExecuteAbility(BallPlayer owner)
        {
            //GameObject beam = owner.GetBaseWeapon.transform.GetChild(0).gameObject;
            //beam.SetActive(true);
            
        }

    }
}

