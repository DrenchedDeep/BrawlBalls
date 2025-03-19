using Gameplay;
using Stats;
using Unity.Netcode;
using UnityEngine;


public interface IWeaponComponent
{
    void Init(BallPlayer owner);
    void Fire(WeaponStats stats, out Vector3 velocity);
    void FireDummy(WeaponStats stats, Vector3 velocity);

    //... what other funcs should this have?
}
