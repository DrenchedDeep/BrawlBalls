using Managers.Network;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Caltrop : Ability
    {
        public override bool CanUseAbility(BallPlayer owner)
        {
            //Not possible to fail...
            return true;
        }

        protected override void UseAbility(BallPlayer owner)
        {
            NetworkGameManager.Instance.SpawnObjectGlobally_ServerRpc("Caltrop", owner.transform.GetChild(0).position, Quaternion.identity);
        }
    }
}
