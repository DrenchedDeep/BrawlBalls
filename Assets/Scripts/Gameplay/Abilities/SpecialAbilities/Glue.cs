using Managers;
using Managers.Local;
using Managers.Network;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Glue : Ability
    {

        public override bool CanUseAbility(BallPlayer owner)
        {
            return Physics.Raycast(owner.transform.GetChild(0).position, Vector3.down, 1, StaticUtilities.GroundLayers);
        }

        protected override void UseAbility(BallPlayer owner)
        {
            NetworkGameManager.Instance.SpawnObjectGlobally_ServerRpc("Glue", owner.transform.GetChild(0).position, Quaternion.identity);
        }
    }
}
