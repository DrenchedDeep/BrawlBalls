
using Gameplay;

public class Abductor : Ability
{
    public override bool CanUseAbility(Ball owner, Weapon weapon)
    {
        return true;
    }

    protected override void UseAbility(Ball owner, Weapon weapon)
    {
        weapon.MultiplyDamage(-1);
    }
}
