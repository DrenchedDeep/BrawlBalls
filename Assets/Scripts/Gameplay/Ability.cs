namespace Gameplay
{
    public abstract class Ability // Cringe AF, need to expose Mono...
    {
        //Trust that our owner will properly dispose of us...
        public bool ActivateAbility(Ball owner, Weapon weapon)
        {
            if (!CanUseAbility(owner, weapon)) return false;
            UseAbility(owner, weapon);
            return true;
        }

        public abstract bool CanUseAbility(Ball owner, Weapon weapon);
        protected abstract void UseAbility(Ball owner, Weapon weapon);
    }
}
