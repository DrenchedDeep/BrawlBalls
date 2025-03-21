namespace Gameplay.Abilities.WeaponAbilities
{
    public class SelfDestructButton : Ability
    {
        public override bool CanUseAbility(BallPlayer owner) => true;

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
