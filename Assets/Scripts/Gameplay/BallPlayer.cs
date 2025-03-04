using System;
using Managers.Local;
using Managers.Network;
using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay
{
    public class BallPlayer : NetworkBehaviour, IDamageAble
    {
        public Ball GetBall {get; private set;}
        public Weapon GetWeapon { get; private set; }
        public AbilityStats GetAbility { get; private set; }
        public bool IsAlive => _currentHealth > 0;
        public float Mass => _rb.mass;

        private Rigidbody _rb;

        public event Action<ulong> OnDestroyed;

        private float _currentHealth;
        private ulong _previousAttackerID;


        [ClientRpc]
        public void Initialize_ClientRpc(string abilityId)
        {
            GetAbility = ResourceManager.Abilities[abilityId];
            GetBall = GetComponentInChildren<Ball>();
            GetWeapon = GetComponentInChildren<Weapon>();
            _rb = GetComponent<Rigidbody>();
            _rb.mass = GetBall.Mass + GetWeapon.Mass;
            _currentHealth = GetBall.MaxHealth;
            
            gameObject.layer = IsOwner?StaticUtilities.LocalBallLayerLiteral:StaticUtilities.EnemyLayerLiteral;

            foreach (Transform child in transform)
            {
                child.gameObject.layer = gameObject.layer;
            }
            
            
            
            if (IsOwner)
              LocalPlayerController.LocalBallPlayer.BindTo(this);
        }
        
        [ServerRpc]
        private void Die_ServerRpc(ulong killer)
        {
            //previousAttacker.AwardKill();
            //Instantiate(onDestroy,transform.position,previousAttacker.transform.rotation);
            //Because it needs to be parent last :(
            NetworkGameManager.Instance.PlayParticleGlobally_ServerRpc("Confetti", transform.position, transform.rotation);
            OnDestroyed?.Invoke(killer);
            transform.GetChild(1).GetComponent<NetworkObject>().Despawn();
            transform.GetChild(2).GetComponent<NetworkObject>().Despawn();
            NetworkObject.Despawn();
        }

        [ClientRpc]
        public void TakeDamage_ClientRpc(float amount, Vector3 direction, ulong attacker)
        {
            print("Check damage: " + amount);
            if (!IsOwner) return;
            
            _currentHealth -= amount;
            
            print(name + "Ouchie! I took damage: " + amount + ",  " + direction + ", I have reamining health: " + _currentHealth);
          
            if (_currentHealth <= 0)
            {
                _previousAttackerID = attacker;
                _ = MessageManager.Instance.HandleScreenMessage("Died to: <color=#ff000>" + attacker + "</color>", 3f);
                Debug.LogWarning("TODO: When we die, let's look at our attacker for a bit.");
                Die_ServerRpc(attacker);
                return;
            }

            _rb.AddForce(direction, ForceMode.Impulse);
        }

    }
}