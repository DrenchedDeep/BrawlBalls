using UnityEngine;

namespace Gameplay.Abilities.WeaponAbilities
{
    public class Cannon : Ability
    {
        private Parabola _parabola;
        private float _firePower;
        
        public override bool CanUseAbility(BallPlayer owner)
        {
            return true;
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
