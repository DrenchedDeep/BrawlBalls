using Gameplay.Abilities.SpecialAbilities;
using UnityEngine;

namespace Gameplay.Abilities.WeaponAbilities
{
    public class Laserbeam : Ability
    {
        public override  bool CanUseAbility(BallPlayer owner)
        {
            //If the beam is not being used AND the beam is not on cooldown...
            //If the beam is being used, it is inherently on cooldown...

            //Therefore, we just return true...
            //Preferably sometime in the future, the "indicator" goes down slowly, then up slowly on sep. timers..
            return true;

        }

        protected override void UseAbility(BallPlayer owner)
        {
            GameObject beam = owner.GetWeapon.transform.GetChild(0).gameObject;
            beam.SetActive(true);
            
        }

    }
}

