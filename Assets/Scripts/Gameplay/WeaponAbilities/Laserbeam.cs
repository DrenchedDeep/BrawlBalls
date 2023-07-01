using System;
using System.Collections;
using UnityEngine;

namespace Gameplay.WeaponAbilities
{
    public class Laserbeam : Ability
    {
        private static readonly WaitForSeconds Duration = new (3);
        protected override bool CanUseAbility(Ball owner, Weapon weapon, out string failText)
        {
            //If the beam is not being used AND the beam is not on cooldown...
            //If the beam is being used, it is inherently on cooldown...

            //Therefore, we just return true...
            //Preferably sometime in the future, the "indicator" goes down slowly, then up slowly on sep. timers..
            failText = "";
            return true;

        }

        protected override void UseAbility(Ball owner, Weapon weapon)
        {
            weapon.StartCoroutine(BeginBeam());
        }

        //Run this on our owner
        private IEnumerator BeginBeam()
        {
            yield return null;
        }

    }
}
