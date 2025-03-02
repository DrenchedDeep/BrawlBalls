using Gameplay.Balls;
using Managers;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Portal : Ability
    {
        public override bool CanUseAbility(NetworkBall owner, Weapon weapon)
        {
            return Physics.CheckSphere(owner.transform.GetChild(0).position, 1, StaticUtilities.GroundLayers); 
        }

        protected override void UseAbility(NetworkBall owner, Weapon weapon)
        {
            Level.Level.Instance.SpawnObjectGlobally_ServerRpc("Portal", owner.transform.GetChild(0).position);
        }
    }
}
