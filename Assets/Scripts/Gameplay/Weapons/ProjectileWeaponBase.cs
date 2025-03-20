using System;
using Gameplay.Weapons;
using Unity.Netcode;
using UnityEngine;

public class ProjectileWeaponBase : BaseWeapon
{
    [SerializeField] protected ProjectileWeapon[] projectileWeapons;

    public override void Start()
    {
        base.Start();

        foreach (ProjectileWeapon wpn in projectileWeapons)
        {
            wpn.Init(Owner);
        }
    }

    protected override void Attack()
    {
        base.Attack();
        
        for (int i = 0; i < projectileWeapons.Length; i++)
        {
            //fire locally
            projectileWeapons[i].Fire(stats, out Vector3 velocity);
            
            //tell server to spawn projectiles for every other clients
            if (NetworkManager.Singleton)
            {
                Attack_ServerRpc(i, velocity);
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    protected void Attack_ServerRpc(int index, Vector3 velocity) => Attack_ClientRpc(index, velocity);

    [ClientRpc(RequireOwnership = false)]
    protected void Attack_ClientRpc(int index, Vector3 velocity)
    {
        if (!IsOwner)
        {
            projectileWeapons[index].FireDummy(stats, velocity);
        }
    }
}
