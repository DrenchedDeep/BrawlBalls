using UnityEngine;

public abstract class Ability // Cringe AF, need to expose Mono...
{
    //Trust that our owner will properly dispose of us...
    public bool ActivateAbility(Ball owner, Weapon weapon, out string failText)
    {
        if (!CanUseAbility(owner, weapon, out failText)) return false;
        UseAbility(owner, weapon);
        return true;
    }

    protected abstract bool CanUseAbility(Ball owner, Weapon weapon, out string failText);
    protected abstract void UseAbility(Ball owner, Weapon weapon);
}
