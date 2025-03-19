using Managers.Network;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Jump : Ability
    {
        public override bool CanUseAbility(BallPlayer owner) => true;

        public override void ExecuteAbility(BallPlayer owner)
        {
            owner.GetBall.ChangeVelocity(50 * Vector3.up);
            NetworkGameManager.Instance.PlayParticleGlobally_ServerRpc("Jump", owner.transform.position, Quaternion.identity);
        }

        public override void CancelAbility(BallPlayer owner)
        {
            throw new System.NotImplementedException();
        }
    }
}
