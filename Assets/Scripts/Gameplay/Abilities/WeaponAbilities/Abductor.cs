namespace Gameplay.Abilities.WeaponAbilities
{
    public class Abductor : Ability
    {
        public override bool CanUseAbility(BallPlayer owner)
        {
            return true;
        }

        public override void ExecuteAbility(BallPlayer owner)
        {
            //float d = owner.GetBaseWeapon.MultiplyDamage(-1);
            //LaserEffectHandler beam =owner.GetBaseWeapon.transform.GetChild(0).GetComponent<LaserEffectHandler>();
            //beam.gameObject.SetActive(true);
            //beam.SetProperty(StaticUtilities.SpeedID, d);
            //beam.SetProperty(StaticUtilities.NoiseSpeedID, d*2);
        }
    }
}
