using System;
using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Weapons
{
    public class CannonWeapon : ProjectileWeaponBase
    {
        private static readonly int Charge = Animator.StringToHash("Charging");
        private static readonly int Launch = Animator.StringToHash("Launch");

        [SerializeField] private Parabola parabola;
        [SerializeField] private float minFirePowerBeforeAttack = 0.2f;
        [SerializeField] private float maxFirePower = 20f;
        [SerializeField] private float firePowerMultiplier = 2f;
        [SerializeField] private float chargeRate = 10f; 

        [SerializeField] private Animator animator;

        [SerializeField] private ProjectileStats TEMP_ProjectileStast;

        private void Awake()
        {
            parabola.BindProjectile(TEMP_ProjectileStast);
            Debug.LogError("TODO AFTER GAMECON: Optimize pool.");
        }

        protected override void OnChargeStart()
        {
            base.OnChargeStart();
            _currentChargeUp  = 0.0f;
            animator.SetBool(Charge, true);
            parabola.ToggleLineRenderer(true);
        }

        protected override void OnChargeStop()
        {
            base.OnChargeStop();
            Debug.Log($"Rocket charge stop: _currentChargeUp:{_currentChargeUp}, minFirePowerBeforeAttack:{minFirePowerBeforeAttack}");
            if (_currentChargeUp >= minFirePowerBeforeAttack)
            {
                Debug.Log("Rocket RELEASE attack");
                Attack();
                return;
            }
            animator.SetBool(Charge, false);
            parabola.ToggleLineRenderer(false);
            _currentChargeUp = 0.0f;
        }

        protected override void Fire()
        {
            ProjectileWeaponStats ps = (ProjectileWeaponStats)stats;
            for (int i = 0; i < projectileWeapons.Length; i++)
            {
                //fire locally
                Debug.Log($"Launching Rocket with velocity: {parabola.FirePower }*{ parabola.PS.InitialVelocity} = {parabola.FirePower * parabola.PS.InitialVelocity}");
                Vector3[] vels =projectileWeapons[i].Fire(ps, parabola.FirePower * parabola.PS.InitialVelocity);
                
                //tell server to spawn projectiles for every other clients
                if (NetworkManager.Singleton)
                {
                    Attack_ServerRpc(i, vels);
                }
            }
        }

        protected override void Attack()
        {
            animator.SetBool(Charge, false);
            parabola.ToggleLineRenderer(false);
            _currentChargeUp = 0.0f;
            animator.SetTrigger(Launch);

            base.Attack();
        }
    
        protected override void ChargeUpTick(float deltaTime)
        {
            parabola.FirePower=(Mathf.Min(_currentChargeUp * chargeRate, maxFirePower));
        }
    }
}
