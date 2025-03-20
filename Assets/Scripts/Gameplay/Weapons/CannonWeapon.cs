using System;
using Gameplay.Weapons;
using Unity.Netcode;
using UnityEngine;

public class CannonWeapon : ProjectileWeaponBase
{
    [SerializeField] private Parabola parabola;
    [SerializeField] private float minFirePowerBeforeAttack = 0.2f;
    [SerializeField] private float maxFirePower = 20f;
    [SerializeField] private float chargeRate = 10f; 

    private bool _updateParabola;
    private float _firePower;
    
    public override void AttackStart()
    {
        Debug.Log("attack start???");

        _updateParabola = true;
        parabola.ToggleLineRenderer(true);
    }

    public override void AttackEnd()
    {
        Debug.Log("attack end???");
        if (_firePower >= minFirePowerBeforeAttack)
        {
            Attack();
        }
        
        _updateParabola = false;
        parabola.ToggleLineRenderer(false);

        _firePower = 0.0f;
    }

    protected override void Attack()
    {
        for (int i = 0; i < projectileWeapons.Length; i++)
        {
            //fire locally
            projectileWeapons[i].Fire(stats, out Vector3 velocity, _firePower);
            
            //tell server to spawn projectiles for every other clients
            if (NetworkManager.Singleton)
            {
                Attack_ServerRpc(i, velocity);
            }
        }
    }
    
    private void Update()
    {
        if (IsOwner)
        {
        }

        if (_updateParabola)
        {
            _firePower += Time.deltaTime * chargeRate;
            _firePower = Mathf.Clamp(_firePower, 0f, maxFirePower);
        }
        
        Debug.Log(_firePower);
        
        parabola.UpdateFirePower(_firePower);
        
    }
}
