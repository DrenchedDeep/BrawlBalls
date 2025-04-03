using UnityEngine;

namespace Gameplay.Abilities.WeaponAbilities
{
    public class Laser : Ability
    {
        //override if need be :P
        public override bool CanUseAbility(BallPlayer owner)
        {
            return owner.GetBaseWeapon.CanAttack();
        }

        public override void ExecuteAbility(BallPlayer owner)
        {
            owner.GetBaseWeapon.AttackStart();
        }

        public override void CancelAbility(BallPlayer owner)
        {
            owner.GetBaseWeapon.AttackEnd();
        }
    }
}

