using Gameplay.Balls;
using Managers;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Glue : Ability
    {

        public override bool CanUseAbility(NetworkBall owner, Weapon weapon)
        {
            return Physics.Raycast(owner.transform.GetChild(0).position, Vector3.down, 1, StaticUtilities.GroundLayers);
        }

        protected override void UseAbility(NetworkBall owner, Weapon weapon)
        {
            Level.Level.Instance.SpawnObjectGlobally_ServerRpc("Glue", owner.transform.GetChild(0).position);
        }
    }
}
