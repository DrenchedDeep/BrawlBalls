using Gameplay.Balls;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Caltrop : Ability
    {
        public override bool CanUseAbility(NetworkBall owner, Weapon weapon)
        {
            //Not possible to fail...
            return true;
        }

        protected override void UseAbility(NetworkBall owner, Weapon weapon)
        {
            Level.Level.Instance.SpawnObjectGlobally_ServerRpc("Caltrop", owner.transform.GetChild(0).position);
        }
    }
}
