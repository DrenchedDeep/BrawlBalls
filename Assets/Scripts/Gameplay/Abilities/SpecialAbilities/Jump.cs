using Managers.Network;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Jump : Ability
    {
        public override bool CanUseAbility(BallPlayer owner) => true;

        public override void ExecuteAbility(BallPlayer owner)
        {
            owner.GetBall.RigidBody.AddForce(1500 * Vector3.up, ForceMode.Impulse);
     //       NetworkGameManager.Instance.PlayParticleGlobally_ServerRpc("Jump", owner.transform.position, Quaternion.identity);
        }

        public override void CancelAbility(BallPlayer owner)
        {
        }
    }
}
