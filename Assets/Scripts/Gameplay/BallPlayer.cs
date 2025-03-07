﻿using System;
using Managers.Local;
using Managers.Network;
using Stats;
using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Gameplay
{
            
    public struct DamageProperties : INetworkSerializable
    {
        public float Damage;
        public Vector3 Direction;
        public ulong Attacker;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Damage);
            serializer.SerializeValue(ref Direction);
            serializer.SerializeValue(ref Attacker);
        }

        public DamageProperties(float damage, Vector3 direction, ulong attacker)
        {
            Damage = damage;
            Direction = direction;
            Attacker = attacker;
        }
    }
    
    public class BallPlayer : NetworkBehaviour, IDamageAble
    {
        public Ball GetBall { get; private set; }
        public Weapon GetWeapon { get; private set; }
        public AbilityStats GetAbility { get; private set; }
        public bool IsAlive => _currentHealth.Value > 0;
        public float Mass => _rb.mass;

        private Rigidbody _rb;

        public event Action<ulong> OnDestroyed;
        
        
        //getters for NetworkedVariables
        public float CurrentHealth => _currentHealth.Value;
        public ulong PreviousAttackerID => _previousAttackerID.Value;

        private readonly NetworkVariable<float> _currentHealth = new(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<ulong> _previousAttackerID = new NetworkVariable<ulong>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _currentHealth.OnValueChanged += OnDamageTaken;
        }

        
        [ClientRpc]
        public void Initialize_ClientRpc(string abilityId)
        {
            Debug.Log("We are now initialized", gameObject);
            GetAbility = ResourceManager.Abilities[abilityId];
            GetBall = GetComponentInChildren<Ball>();
            GetWeapon = GetComponentInChildren<Weapon>();

            _rb = GetComponent<Rigidbody>();
            _rb.mass = GetBall.Mass + GetWeapon.Mass;

            gameObject.layer = IsOwner ? StaticUtilities.LocalBallLayerLiteral : StaticUtilities.EnemyLayerLiteral;

            foreach (Transform child in transform)
            {
                child.gameObject.layer = gameObject.layer;
            }

            if (IsHost) _currentHealth.Value = GetBall.MaxHealth;


            if (IsOwner) LocalPlayerController.LocalBallPlayer.BindTo(this);
        }

        
        /*/
        [ServerRpc]
        public void Die_ServerRpc(ulong killer)
        {
            //previousAttacker.AwardKill();
            //Instantiate(onDestroy,transform.position,previousAttacker.transform.rotation);
            //Because it needs to be parent last :(
            
            Die(killer);
        }
        public void Die(ulong killer)
        {
            NetworkGameManager.Instance.PlayParticleGlobally_ServerRpc("Confetti", transform.position,
                transform.rotation);
            
            Die_ClientRpc(killer);
        }

        //need to let the local player know when they die so they can respawn themselves :D
        [ClientRpc]
        public void Die_ClientRpc(ulong killer)
        {
            OnDestroyed?.Invoke(killer);
            ActualDie_ServerRpc(killer);
        }

        [ServerRpc]
        public void ActualDie_ServerRpc(ulong killer)
        {
            OnDestroyed?.Invoke(killer);
            transform.GetChild(0).GetComponent<NetworkObject>().Despawn();
            transform.GetChild(1).GetComponent<NetworkObject>().Despawn();
            NetworkObject.Despawn();

        }
        

        [ClientRpc]
        public void TakeDamage_ClientRpc(float amount, Vector3 direction, ulong attacker)
        {
            print("Check damage: " + amount);
            if (!IsServer) return;

            _currentHealth.Value -= amount;

            print(name + "Ouchie! I took damage: " + amount + ",  " + direction + ", I have reamining health: " +
                  _currentHealth);

            if (_currentHealth.Value <= 0)
            {
                _previousAttackerID.Value = attacker;
                _ = MessageManager.Instance.HandleScreenMessage("Died to: <color=#ff000>" + attacker + "</color>", 3f);
                Debug.LogWarning("TODO: When we die, let's look at our attacker for a bit.");
                Die_ServerRpc(attacker);
                return;
            }

            _rb.AddForce(direction, ForceMode.Impulse);
        }
        /*/

        [ServerRpc(RequireOwnership = false)]
        public void TakeDamage_ServerRpc(DamageProperties damageInfo)
        {
            print("TAKING DAMAGE ON THE SERVER: " + damageInfo.Damage);

            _currentHealth.Value -= damageInfo.Damage;
            
            print(name + "Ouchie! I took damage: " + damageInfo.Damage + ",  " + damageInfo.Direction + ", I have reamining health: " +
                  _currentHealth);

            if (_currentHealth.Value <= 0)
            {
                _previousAttackerID.Value = damageInfo.Attacker;
                _ = MessageManager.Instance.HandleScreenMessage("Died to: <color=#ff000>" + damageInfo.Attacker + "</color>", 3f);
                Die_Server(damageInfo.Attacker);
                return;
            }
            
            _rb.AddForce(damageInfo.Direction, ForceMode.Impulse);
        }

        //called on the SERVER
        public void Die_Server(ulong killer)
        {
            NetworkGameManager.Instance.PlayParticleGlobally_ServerRpc("Confetti", transform.position,
                transform.rotation);

            _currentHealth.Value = 0;
            Die_ClientRpc(killer);
        }
        
        //doing this cuz the client needs to verify they know that their ball has died before we can despawn it... probs a better work around but for now this will work and given time constraints MAKE IT WORK! 
        [ClientRpc]
        public void Die_ClientRpc(ulong killer)
        {
            OnDestroyed?.Invoke(killer);
            ActualDie_ServerRpc(killer);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void ActualDie_ServerRpc(ulong killer)
        {
            if (!IsHost)
            {
                OnDestroyed?.Invoke(killer);
            }

            NetworkGameManager.Instance.IncreasePlayerScore(killer);
            transform.GetChild(0).GetComponent<NetworkObject>().Despawn();
            transform.GetChild(1).GetComponent<NetworkObject>().Despawn();
            NetworkObject.Despawn();

        }
        

        //called on all? clients when the server updates the health
        public void OnDamageTaken(float old, float current)
        {
            //play vfx, sfx, whatever here

            if (current <= 0)
            {
                //play confetti particles here??
            }
        }
    }
}