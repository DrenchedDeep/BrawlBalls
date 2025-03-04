namespace Gameplay
{
    public abstract class Ability // Cringe AF, must be abstract class for reflection in stats.
    {
        //Trust that our owner will properly dispose of us...
        public bool ActivateAbility(BallPlayer owner)
        {
            if (!CanUseAbility(owner)) return false;
            UseAbility(owner);
            return true;
        }

        public abstract bool CanUseAbility(BallPlayer owner);
        
        
        //DO NOT CALL THIS FUNCTION, IT IS AUTOMATICALLY MANAGED.
        protected abstract void UseAbility(BallPlayer owner);
    }
}
