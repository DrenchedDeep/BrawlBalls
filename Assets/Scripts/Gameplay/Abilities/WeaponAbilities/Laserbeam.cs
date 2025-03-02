using Gameplay.Abilities.SpecialAbilities;
using Gameplay.Balls;
using UnityEngine;

namespace Gameplay.Abilities.WeaponAbilities
{
    public class Laserbeam : Ability
    {
        public override  bool CanUseAbility(NetworkBall owner,  Weapon weapon)
        {
            //If the beam is not being used AND the beam is not on cooldown...
            //If the beam is being used, it is inherently on cooldown...

            //Therefore, we just return true...
            //Preferably sometime in the future, the "indicator" goes down slowly, then up slowly on sep. timers..
            return true;

        }

        protected override void UseAbility(NetworkBall owner, Weapon weapon)
        {
            GameObject beam = weapon.transform.GetChild(0).gameObject;
            beam.SetActive(true);
            
        }

    }
}

