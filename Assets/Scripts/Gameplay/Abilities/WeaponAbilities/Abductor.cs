using Gameplay.Abilities.SpecialAbilities;
using Managers;
using Managers.Local;
using MarkedForDeath;

namespace Gameplay.Abilities.WeaponAbilities
{
    public class Abductor : Ability
    {
        public override bool CanUseAbility(BallPlayer owner)
        {
            return true;
        }

        protected override void UseAbility(BallPlayer owner)
        {
            float d = owner.GetWeapon.MultiplyDamage(-1);
            LaserEffectHandler beam =owner.GetWeapon.transform.GetChild(0).GetComponent<LaserEffectHandler>();
            beam.gameObject.SetActive(true);
            beam.SetProperty(StaticUtilities.SpeedID, d);
            beam.SetProperty(StaticUtilities.NoiseSpeedID, d*2);
        }
    }
}
