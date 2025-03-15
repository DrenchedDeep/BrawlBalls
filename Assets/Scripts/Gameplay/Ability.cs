namespace Gameplay
{
    public abstract class Ability // Cringe AF, must be abstract class for reflection in stats.
    {
        public abstract bool CanUseAbility(BallPlayer owner);
        public abstract void ExecuteAbility(BallPlayer owner);


    }
}
