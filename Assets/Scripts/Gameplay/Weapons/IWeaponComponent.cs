using Gameplay;
using Stats;
using Unity.Netcode;
using UnityEngine;


public interface IWeaponComponent
{
    void Init(BallPlayer owner);
    void Fire(WeaponStats stats);
    
    //... what other funcs should this have?
}
