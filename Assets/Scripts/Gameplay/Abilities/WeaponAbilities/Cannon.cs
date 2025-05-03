using Managers.Local;
using UnityEngine;

namespace Gameplay.Abilities.WeaponAbilities
{
    public class Cannon : Ability
    {
        public override bool CanUseAbility(BallPlayer owner)
        {
            return true;
        }

        public override void ExecuteAbility(BallPlayer owner)
        {
            owner.GetBaseWeapon.AttackStart();
            //AudioManager.instance.PlayOneShot();
        }
        
        public override void CancelAbility(BallPlayer owner)
        {
            owner.GetBaseWeapon.AttackEnd();
        }
    }
}
