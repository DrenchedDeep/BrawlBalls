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
    
        private bool _updateParabola;
        private float _firePower;
    
        public override void AttackStart()
        {
            Debug.Log("ATTACK START!");
            animator.SetBool(Charge, true);
            _updateParabola = true;
            parabola.ToggleLineRenderer(true);
        }

        public override void AttackEnd()
        {

            if (_firePower >= minFirePowerBeforeAttack)
            {
                Attack();
            }
            animator.SetBool(Charge, false);
        
            _updateParabola = false;
            parabola.ToggleLineRenderer(false);

            _firePower = 0.0f;
        }

        protected override void Attack()
        {
            animator.SetTrigger(Launch);

            for (int i = 0; i < projectileWeapons.Length; i++)
            {
                //fire locally
                projectileWeapons[i].Fire(stats, out Vector3 velocity, _firePower * firePowerMultiplier);
            
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
        
            parabola.UpdateFirePower(_firePower);
        
        }
    }
}
