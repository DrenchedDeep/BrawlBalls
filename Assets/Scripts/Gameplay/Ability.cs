using UnityEngine;

public abstract class Ability // Cringe AF, need to expose Mono...
{
    protected Ball MyOwner;
    protected Weapon MyWeapon; //Only for weapons
    
    //Trust that our owner will properly dispose of us...
    public void ActivateAbility(Ball owner, Weapon weapon)
    {
        MyOwner = owner;
        MyWeapon = weapon;
        UseAbility();
    }

    protected abstract void UseAbility();
}
