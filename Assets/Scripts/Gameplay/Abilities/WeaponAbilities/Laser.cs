using UnityEngine;

namespace Gameplay.Abilities.WeaponAbilities
{
    public class Laser : Ability
    {
        public override bool CanUseAbility(BallPlayer owner) => true;

        public override void ExecuteAbility(BallPlayer owner)
        {
            owner.GetBaseWeapon.Attack();
        }

    }
}

