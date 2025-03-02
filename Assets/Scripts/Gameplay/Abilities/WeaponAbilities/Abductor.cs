using Gameplay.Abilities.SpecialAbilities;
using Gameplay.Balls;
using Managers;
using MarkedForDeath;

namespace Gameplay.Abilities.WeaponAbilities
{
    public class Abductor : Ability
    {
        public override bool CanUseAbility(NetworkBall owner, Weapon weapon)
        {
            return true;
        }

        protected override void UseAbility(NetworkBall owner, Weapon weapon)
        {
            float d =weapon.MultiplyDamage(-1);
            LaserEffectHandler beam = weapon.transform.GetChild(0).GetComponent<LaserEffectHandler>();
            beam.gameObject.SetActive(true);
            beam.SetProperty(StaticUtilities.SpeedID, d);
            beam.SetProperty(StaticUtilities.NoiseSpeedID, d*2);
        }
    }
}
