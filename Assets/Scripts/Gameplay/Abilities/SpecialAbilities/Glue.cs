using Managers.Local;
using Managers.Network;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Glue : Ability
    {

        public override bool CanUseAbility(BallPlayer owner)
        {
            return  owner.GetBall.IsGrounded;
        }

        public override void ExecuteAbility(BallPlayer owner)
        {
            Physics.Raycast(owner.transform.position, Vector3.down, out var hit, 5, StaticUtilities.GroundLayers);
            NetworkGameManager.Instance.SpawnObjectGlobally_ServerRpc("Glue", hit.point, Quaternion.identity);
        }

        public override void CancelAbility(BallPlayer owner)
        {
        }
    }
}
