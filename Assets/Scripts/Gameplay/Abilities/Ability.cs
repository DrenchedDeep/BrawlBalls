using Gameplay.Balls;

namespace Gameplay.Abilities
{
    public abstract class Ability // Cringe AF, must be abstract class for reflection in stats.
    {
        //Trust that our owner will properly dispose of us...
        public bool ActivateAbility(NetworkBall owner, Weapon weapon)
        {
            if (!CanUseAbility(owner, weapon)) return false;
            UseAbility(owner, weapon);
            return true;
        }

        public abstract bool CanUseAbility(NetworkBall owner, Weapon weapon);
        
        
        //DO NOT CALL THIS FUNCTION, IT IS AUTOMATICALLY MANAGED.
        protected abstract void UseAbility(NetworkBall owner, Weapon weapon);
    }
}
