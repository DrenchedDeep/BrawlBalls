using Managers;
using Managers.Local;
using Managers.Network;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Jump : Ability
    {
        public override bool CanUseAbility(BallPlayer owner)
        {
            //Only allow jump if grounded.
            return Physics.Raycast(owner.transform.GetChild(1).position, Vector3.down, 1, StaticUtilities.GroundLayers); 
        }

        protected override void UseAbility(BallPlayer owner)
        {
            owner.GetBall.ChangeVelocity(50 * Vector3.up);
            NetworkGameManager.Instance.PlayParticleGlobally_ServerRpc("Jump", owner.transform.GetChild(1).position, Quaternion.identity);
        }
    }
}
