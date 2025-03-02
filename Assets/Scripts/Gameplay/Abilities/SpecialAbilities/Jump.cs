using Gameplay.Balls;
using Managers;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Jump : Ability
    {
        public override bool CanUseAbility(NetworkBall owner, Weapon weapon)
        {
            //Only allow jump if grounded.
            return Physics.Raycast(owner.transform.GetChild(1).position, Vector3.down, 1, StaticUtilities.GroundLayers); 
        }

        protected override void UseAbility(NetworkBall owner, Weapon weapon)
        {
            owner.ChangeVelocity(50 * Vector3.up);
            Level.Level.Instance.PlayParticleGlobally_ServerRpc("Jump", owner.transform.GetChild(1).position);
        }
    }
}
