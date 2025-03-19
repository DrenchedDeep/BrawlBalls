using Gameplay.Weapons;
using Unity.Netcode;
using UnityEngine;

public class ProjectileWeaponBase : BaseWeapon
{
    [SerializeField] private ProjectileWeapon[] projectileWeapons;
    
    public override void Attack()
    {
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
    void Attack_ServerRpc(int index, Vector3 velocity) => Attack_ClientRpc(index, velocity);

    [ClientRpc(RequireOwnership = false)]
    void Attack_ClientRpc(int index, Vector3 velocity)
    {
        if (!IsOwner)
        {
            projectileWeapons[index].FireDummy(stats, velocity);
        }
    }
}
