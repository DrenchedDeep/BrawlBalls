using System;
using Gameplay.Balls;
using Gameplay.Pools;
using Gameplay.Weapons;
using LocalMultiplayer;
using Managers.Local;
using Managers.Network;
using Stats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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
        public BaseWeapon GetBaseWeapon { get; private set; }
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
        
        [SerializeField] private Transform damageNumberSpawnPoint;

        public PlayerController Owner { get; private set; }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            
            
            _currentHealth.OnValueChanged += OnDamageTaken;

            //default this to impossible number
            if (IsServer)
            {
                _previousAttackerID.Value = 200; 
            }

            NetworkGameManager.Instance.OnGameStateUpdated += OnGameStateUpdated;
        }

        private void OnGameStateUpdated(GameState gameState)
        {
            if (gameState == GameState.InGame)
            {
                _rb.isKinematic = false;
            }
        }
        
        public void Initialize(string abilityID, int playerIndex)
        {
            //server should know bout these aswell
            GetBall = GetComponentInChildren<Ball>();
            GetBaseWeapon = GetComponentInChildren<BaseWeapon>();
            //GetBall.Init(this);
            Initialize_ClientRpc(abilityID, playerIndex);
            
        }


        [ClientRpc]
        public void Initialize_ClientRpc(string abilityId, int playerIndex)
        {
            if (IsOwner)
            {
                Owner = SaveManager.FindPlayerByID(playerIndex).LocalInput.GetComponent<PlayerController>();
                Owner.BindTo(this);
            }
            
            GetAbility = ResourceManager.Abilities[abilityId];
            GetBall = GetComponentInChildren<Ball>();
            GetBaseWeapon = GetComponentInChildren<BaseWeapon>();
            Physics.SyncTransforms();
            _currentHealth.Value = GetBall.Stats.MaxHealth;

            GetBall.Init(this);


            _rb = GetComponent<Rigidbody>();
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            if (NetworkGameManager.Instance.GameState.Value == GameState.SelectingBalls ||
                NetworkGameManager.Instance.GameState.Value == GameState.StartingGame)
            {
                _rb.isKinematic = true;
            }
            else
            {
                _rb.isKinematic = false;
            }
            
            _rb.mass = GetBall.Stats.Mass + GetBaseWeapon.Stats.Mass;

            gameObject.layer = IsOwner ? StaticUtilities.LocalBallLayerLiteral : StaticUtilities.EnemyLayerLiteral;

            foreach (Transform child in transform)
            {
                child.gameObject.layer = gameObject.layer;
            }

            
        }


        #if UNITY_EDITOR
        public void Initialize_Offline(string abilityId, int playerIndex)
        {
            if (NetworkManager.Singleton != null) return;
        
            Owner = SaveManager.FindPlayerByID(playerIndex).LocalInput.GetComponent<PlayerController>();
            Owner.BindTo(this);
            
            
            Debug.Log("We are now initialized", gameObject);
            GetAbility = ResourceManager.Abilities[abilityId];
            GetBall = GetComponentInChildren<Ball>();
            GetBaseWeapon = GetComponentInChildren<BaseWeapon>();
            
            GetBall.Init(this);

            _rb = GetComponent<Rigidbody>();
            _rb.mass = GetBall.Stats.Mass + GetBaseWeapon.Stats.Mass;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.isKinematic = false;
            
            gameObject.layer = IsOwner ? StaticUtilities.LocalBallLayerLiteral : StaticUtilities.EnemyLayerLiteral;

            
            foreach (Transform child in transform)
            {
                child.gameObject.layer = gameObject.layer;
            }
            

            _currentHealth.Value = GetBall.Stats.MaxHealth;
            
            
        }
        #endif

        [ServerRpc(RequireOwnership = false)]
        public void TakeDamage_ServerRpc(DamageProperties damageInfo)
        {
            if (_currentHealth.Value <= 0)
            {
                return;
            }

            if (NetworkGameManager.Instance.GameState.Value > GameState.InGame)
            {
                return;
            }

            //if were protected :D
            if (GetBall.IsProtected.Value)
            {
                return;
            }
            
            _currentHealth.Value -= damageInfo.Damage;
            
            print(name + "Ouchie! I took damage: " + damageInfo.Damage + ",  " + damageInfo.Direction + ", I have reamining health: " +
                  _currentHealth);

            OnDamageTaken_ClientRpc((int)damageInfo.Damage);
            _previousAttackerID.Value = damageInfo.Attacker;

            if (_currentHealth.Value <= 0)
            {
                _ = MessageManager.Instance.HandleScreenMessage("Died to: <color=#ff000>" + damageInfo.Attacker + "</color>", 3f);
                Die_Server(damageInfo.Attacker);
                return;
            }
            
            _rb.AddForce(damageInfo.Direction, ForceMode.Impulse);
        }

        [ClientRpc(RequireOwnership = false)]
        private void OnDamageTaken_ClientRpc(int damage)
        {
            HitDamageNumber hitDamageNumber = 
                ObjectPoolManager.Instance.GetObjectFromPool<HitDamageNumber>("DamageNumber", damageNumberSpawnPoint.position, damageNumberSpawnPoint.rotation);

            if (hitDamageNumber)
            {
                hitDamageNumber.Init(damage);
            }
        }

        //called on the SERVER
        public void Die_Server(ulong killer)
        {
            //cant die if the game has ended...
            if (NetworkGameManager.Instance.GameState.Value > GameState.InGame)
            {
                return;
            }
            
            NetworkGameManager.Instance.PlayParticleGlobally_ServerRpc("Confetti", transform.position,
                transform.rotation);

            if (killer == 100)
            {
                killer = _previousAttackerID.Value;
            }
            
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

            NetworkGameManager.Instance.OnPlayerKilled(NetworkObject.OwnerClientId, killer);

            transform.GetChild(3).GetComponent<NetworkObject>().Despawn();
            transform.GetChild(4).GetComponent<NetworkObject>().Despawn();
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

        public void GiveAward(ParticleManager.ECollectableType type)
        {
            int awardsToGive = 1;

       //     bool isGoldBall = GetBall is GoldBall;
            if (GetBall is GoldBall)
            {
                awardsToGive *= GoldBall.AwardMultiplier;
            }
            
            
            Debug.Log("give awards?? not sure what to do here....");
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
                public void SetAbility(AbilityStats ability)
                {
                    GetAbility = ability;
                }
    }
    
    
}