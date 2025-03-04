using Managers;
using Managers.Local;
using Managers.Network;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Portal : Ability
    {
        public override bool CanUseAbility(BallPlayer owner)
        {
            return Physics.CheckSphere(owner.transform.GetChild(0).position, 1, StaticUtilities.GroundLayers); 
        }

        protected override void UseAbility(BallPlayer owner)
        {
            NetworkGameManager.Instance.SpawnObjectGlobally_ServerRpc("Portal", owner.transform.GetChild(0).position, Quaternion.identity);
        }
    }
}
