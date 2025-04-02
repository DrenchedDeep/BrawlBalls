using System;
using Gameplay.Balls;
using Gameplay.Pools;
using Gameplay.Weapons;
using LocalMultiplayer;
using Managers.Local;
using Managers.Network;
using Stats;
using Unity.Collections;
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
        public int ChildID;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Damage);
            serializer.SerializeValue(ref Direction);
            serializer.SerializeValue(ref Attacker);
            serializer.SerializeValue(ref ChildID);
        }

        public DamageProperties(float damage, Vector3 direction, ulong attacker, int childID)
        {
            Damage = damage;
            Direction = direction;
            Attacker = attacker;
            ChildID = childID;
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
        
        public Rigidbody Rb => _rb;

        public event Action<ulong, int> OnDestroyed;
        public event Action OnDamaged;
        public event Action OnHealed;

        
        //getters for NetworkedVariables
        public float CurrentHealth => _currentHealth.Value;
        public ulong PreviousAttackerID => _previousAttackerID.Value;

        private readonly NetworkVariable<float> _currentHealth = new(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private readonly NetworkVariable<ulong> _previousAttackerID = new NetworkVariable<ulong>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        public NetworkVariable<int> ChildID { get; private set; } = 
            new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        
        [SerializeField] private Transform damageNumberSpawnPoint;
        [SerializeField] private BallPlayerHUD ballPlayerHUD;

        public PlayerController Owner { get; private set; }

        private float _maxHealth;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            
            //default this to impossible number
            if (IsServer)
            {
//                _currentHealth.Value = GetBall.Stats.MaxHealth;
                _previousAttackerID.Value = 200; 
            }

            _currentHealth.OnValueChanged += OnHealthChanged;
            ChildID.OnValueChanged += OnChildIDChanged;


          //  NetworkGameManager.Instance.OnGameStateUpdated += OnGameStateUpdated;
        }

        private void OnEnable()
        {
            NetworkGameManager.Instance.OnGameStateUpdated += OnGameStateUpdated;

        }

        private void OnDisable()
        {
            NetworkGameManager.Instance.OnGameStateUpdated -= OnGameStateUpdated;

        }

        private void OnGameStateUpdated(GameState gameState)
        {
            if (!_rb)
            {
                _rb = GetComponent<Rigidbody>();
            }
            if (gameState == GameState.InGame)
            {
                _rb.isKinematic = false;
            }
        }

        private void OnChildIDChanged(int old, int current)
        {
            if (IsOwner)
            {
                return;
            }

            BallPlayerHUD playerHud =
                Instantiate(ballPlayerHUD.gameObject, transform.position, Quaternion.identity).GetComponent<BallPlayerHUD>();
            if (playerHud)
            {
                playerHud.AttachTo(this);
                
                string playerName = NetworkGameManager.Instance.GetPlayerName(OwnerClientId, current);
                Debug.Log("PLAYER NAME IS: " + playerName);
                playerHud.SetNameTag(playerName);
                
                
            }
        }

        private void OnHealthChanged(float old, float current)
        {
            if (old > current)
            {
                OnDamaged?.Invoke();
            }
            else
            {
                OnHealed?.Invoke();
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
            GetAbility = ResourceManager.Abilities[abilityId];
            GetBall = GetComponentInChildren<Ball>();
            GetBaseWeapon = GetComponentInChildren<BaseWeapon>();
            
            if (IsOwner)
            {
                Owner = SaveManager.FindPlayerByID(playerIndex).LocalInput.GetComponent<PlayerController>();
                ChildID.Value = Owner.PlayerInput.playerIndex;
                Owner.BindTo(this);
                GetBall.Init(this);
            }
            
            Physics.SyncTransforms();

            _maxHealth = GetBall.Stats.MaxHealth;

            if (IsServer)
            {
                _currentHealth.Value = _maxHealth;
            }
            

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

            OnDamageTaken_ClientRpc((int)damageInfo.Damage, damageInfo.Direction);
            _previousAttackerID.Value = damageInfo.Attacker;

            if (_currentHealth.Value <= 0)
            {
                _ = MessageManager.Instance.HandleScreenMessage("Died to: <color=#ff000>" + damageInfo.Attacker + "</color>", 3f);
                Die_Server(damageInfo.Attacker, damageInfo.ChildID);
                return;
            }
            
        }

        [ClientRpc(RequireOwnership = false)]
        private void OnDamageTaken_ClientRpc(int damage, Vector3 direction)
        {
            HitDamageNumber hitDamageNumber = 
                ObjectPoolManager.Instance.GetObjectFromPool<HitDamageNumber>("DamageNumber", damageNumberSpawnPoint.position, damageNumberSpawnPoint.rotation);

            if (hitDamageNumber)
            {
                hitDamageNumber.Init(damage);
            }
            
            _rb.AddForce(direction, ForceMode.Impulse);

        }

        //called on the SERVER
        public void Die_Server(ulong killer, int killerChildID)
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
            Die_ClientRpc(killer, killerChildID);
        }
        
        //doing this cuz the client needs to verify they know that their ball has died before we can despawn it... probs a better work around but for now this will work and given time constraints MAKE IT WORK! 
        [ClientRpc]
        public void Die_ClientRpc(ulong killer, int killerChildID)
        {
            OnDestroyed?.Invoke(killer, killerChildID);
            ActualDie_ServerRpc(killer, killerChildID);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void ActualDie_ServerRpc(ulong killer, int killerChildID)
        {
            if (!IsHost)
            {
                OnDestroyed?.Invoke(killer, killerChildID);
            }

            NetworkGameManager.Instance.OnPlayerKilled(OwnerClientId, ChildID.Value, killer, killerChildID);

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

        public void RestoreHealth()
        {
            _currentHealth.Value = _maxHealth;
        }

        public void IncreaseMaxHealth(float amt)
        {
            _maxHealth *= amt;
        }
        
        
        public void SetAbility(AbilityStats ability)
        {
            GetAbility = ability;
        }
    }
    
    
}