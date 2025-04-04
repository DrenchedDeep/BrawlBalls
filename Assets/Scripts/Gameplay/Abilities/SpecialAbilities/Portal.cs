using Managers.Network;
using Unity.Mathematics;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Portal : Ability
    {
        public override bool CanUseAbility(BallPlayer owner) => owner.GetBall.IsGrounded;

        public override void ExecuteAbility(BallPlayer owner)
        {
            NetworkGameManager.Instance.SpawnObjectGlobally_ServerRpc("Portal", owner.transform.position, quaternion.identity);
        }

        public override void CancelAbility(BallPlayer owner)
        {
        }
    }
}
