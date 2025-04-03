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
    
        private bool _updateParabola;
    
        protected override void OnChargeStart()
        {
            base.OnChargeStart();
            Debug.Log("ATTACK START!");
            _currentChargeUp  = 0.0f;
            animator.SetBool(Charge, true);
            _updateParabola = true;
            parabola.ToggleLineRenderer(true);
        }

        protected override void OnChargeStop()
        {
            base.OnChargeStop();
            if (_currentChargeUp >= minFirePowerBeforeAttack)
            {
                Attack();
            }
            animator.SetBool(Charge, false);
        
            _updateParabola = false;
            parabola.ToggleLineRenderer(false);

            _currentChargeUp = 0.0f;
        }

        protected override void Attack()
        {
            animator.SetTrigger(Launch);

            base.Attack();
        }
    
        protected override void ChargeUpTick(float deltaTime)
        {
            parabola.UpdateFirePower(Mathf.Min(_currentChargeUp * chargeRate, maxFirePower));
        }
    }
}
