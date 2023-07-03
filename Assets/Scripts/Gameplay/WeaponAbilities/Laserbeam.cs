using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

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
            weapon.StartCoroutine(BeginBeam(weapon));
        }

        //Run this on our owner
        private IEnumerator BeginBeam(Weapon w)
        {
            //We can create show the effects range and capabilities...
            GameObject beam = w.transform.GetChild(0).gameObject;
            beam.SetActive(true);
            
            //On awake, it'll play the intro animation for X seconds...
            
            //(Set the width of the beam)
            //(Set this distance of the beam)
            //(set duration here)
            
            //Then set the weapon to active.
            w.ToggleActive();
            yield return Duration;
            //If w is not null.
            if(w)
                w.ToggleActive();
            
            beam.SetActive(false);
            
            yield return null;
        }

    }
}
