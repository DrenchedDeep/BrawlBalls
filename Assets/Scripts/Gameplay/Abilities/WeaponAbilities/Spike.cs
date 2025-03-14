using System.Collections;
using Managers.Local;
using UnityEngine;

namespace Gameplay.Abilities.WeaponAbilities
{
    public class Spike : Ability
    {
        public override bool CanUseAbility(BallPlayer owner)
        {
            return owner.GetBall.Speed > 3;
        }

        protected override void UseAbility(BallPlayer owner)
        {
            Debug.Log("Attacked!");
            //Un parent self
            owner.GetWeapon.Disconnect(owner.GetBall.Speed);
        
        }
    
        public static IEnumerator Move(Weapon weapon, float speed)
        {
            Transform ownerTrans = weapon.transform;
            float duration = 5;
            while (duration > 0)
            {
                duration -= Time.deltaTime;
                ownerTrans.position += speed * Time.deltaTime * ownerTrans.forward;
                if (Physics.Raycast(ownerTrans.position, ownerTrans.forward, out RaycastHit hit, weapon.Stats.Range.y * 10, StaticUtilities.GroundLayers)) // 1==Default
                {
                    ownerTrans.position = hit.point - ownerTrans.forward * (weapon.Stats.Range.y * 10f);
                    weapon.enabled = false;
                    yield break;
                }
                yield return null;
            }
            weapon.NetworkObject.Despawn();
        }
    }
}
