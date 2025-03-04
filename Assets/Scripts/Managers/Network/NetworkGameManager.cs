using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using MainMenu.UI;
using Managers.Local;
using Unity.Netcode;
using UnityEngine;

namespace Managers.Network
{
    public class NetworkGameManager: NetworkBehaviour
    {
        [SerializeField, Min(0)] private float matchTime = 300;
        [SerializeField, Min(0)] private float timeToMatchStart = 15;
        [SerializeField,  Min(0)] private float matchOverTimeDuration = 120;

        private float _totalTimePassed;
        private float _currentTime;
        private float _currentTimePeriod;
        
        public static NetworkGameManager Instance { get; private set; }
        private readonly HashSet<ulong> _players = new();

        public event Action OnHostDisconnected;
        public event Action OnAllPlayersJoined;
        public event Action OnGameBegin; 
        
        private CancellationTokenSource _matchCancelTokenSource;

        private readonly SortedList<float, Action> _timedMatchEvents = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Start()
        {
         
            enabled = IsServer;
          
            
            CheckGameStart_ServerRpc();
        }
        
        


        #region Connection
        private void OnEnable()
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.OnServerStopped += OnServerStopped;
        }
        private void OnDisable()
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.OnServerStopped -= OnServerStopped;
        }
        void OnClientConnected(ulong clientId)
        {
            Debug.LogWarning("Client connected " + clientId);
        }
        void OnClientDisconnected(ulong clientId)
        {
            Debug.LogWarning("Client Disconnected" + clientId);
            _players.Remove(clientId);
            CheckStartGame();
        }
        
        private void OnServerStopped(bool obj)
        {
            Debug.LogError("Server stopped IMPLEMENT RETURN TO MENU OR HOST MIGRATION. What is this bool?" + obj);
            OnHostDisconnected?.Invoke();
        }
        
        void CheckStartGame()
        {
            print("Checking players connected: " + _players.Count + " == " + NetworkManager.ConnectedClients.Count);
            if (_players.Count == NetworkManager.ConnectedClients.Count)
            {
                StartGame_ClientRpc();
            }
        }
        #endregion
        
        [ServerRpc(RequireOwnership = false)]
        private void CheckGameStart_ServerRpc(ServerRpcParams @params = default)
        {
            Debug.Log("Player has connected and been registered: " + @params.Receive.SenderClientId);

            _players.Add(@params.Receive.SenderClientId);
            CheckStartGame();
        }
        
        
        [ServerRpc(RequireOwnership = false)]
        public void PlayParticleGlobally_ServerRpc(string particleName, Vector3 location, Quaternion rotation) => PlayParticleGlobally_ClientRpc(particleName, location,rotation);
    
                
        [ClientRpc]
        private void PlayParticleGlobally_ClientRpc(string particleName, Vector3 location, Quaternion rotation) => ParticleManager.InvokeParticle(particleName, location, rotation);

        
        [ServerRpc(RequireOwnership = false)]
        public void SpawnObjectGlobally_ServerRpc(string objectName, Vector3 location, Quaternion rotation, ServerRpcParams @params = default)
        {
            NetworkObject ngo = Instantiate(ResourceManager.SummonableObjects[objectName], location, rotation);
            ngo.SpawnWithOwnership(@params.Receive.SenderClientId);
        }

        [ClientRpc]
        public void SendMessage_ClientRpc(string s, float d, ClientRpcParams x = default) => _ = MessageManager.Instance.HandleScreenMessage(s, d);

        
        [ClientRpc]
        private void StartGame_ClientRpc()
        {
            Debug.LogWarning("Starting game... Is the lobby locked?");
            
            LoadingHelper.Deactivate();
            OnAllPlayersJoined?.Invoke();
            
            //Get cancellation token for this 
            _matchCancelTokenSource =  new CancellationTokenSource();
            _ = ManagerMatchTimer(_matchCancelTokenSource.Token);
        }
        
        private async UniTask ManagerMatchTimer(CancellationToken token)
        {
            await ProcessTimeFrame(timeToMatchStart, token);
            
            if (token.IsCancellationRequested)
            {
                Debug.LogWarning("MATCH IS BEING CANCELLED DURING INIT");
                return;
            }
            
            Debug.Log("Game is now beginning fully! Let the games begin!");
            OnGameBegin?.Invoke();
            
            await ProcessTimeFrame(matchTime, token);
            
            if (token.IsCancellationRequested)
            {
                Debug.LogWarning("MATCH IS BEING CANCELLED DURING GAMEPLAY");
                return;
            }
            
            Debug.Log("Now Entering overtime!");
            
            await ProcessTimeFrame(matchOverTimeDuration, token);
            
            if (token.IsCancellationRequested)
            {
                Debug.LogWarning("MATCH IS BEING CANCELLED DURING END GAME");
                return;
            }
            
            Debug.LogWarning("Well I guess everyone who's alive is a winner! TODO: Actually make this work");
        }

        private async UniTask ProcessTimeFrame(float duration, CancellationToken token)
        {
            _currentTimePeriod = duration;
            while (_currentTime < _currentTimePeriod)
            {
                float dt = Time.deltaTime;
                _currentTime += dt;
                _totalTimePassed += dt;

                if (_timedMatchEvents.Count != 0 && GetTotalTimePassed >= _timedMatchEvents.Keys[0])
                {
                    Debug.Log("Executing a timed event at time: " + GetTotalTimePassed);
                    _timedMatchEvents.Remove(_timedMatchEvents.Keys[0]);
                    _timedMatchEvents.Values[0].Invoke();
                }
                await UniTask.Yield(token);
            }
            _currentTime = 0;
        }

        public float EvaluateCurrentTimePeriodAsPercent => _currentTime / _currentTimePeriod;
        public float GetRemainingSectionTime => _currentTimePeriod - _currentTime;
        public float GetRemainingTime => GetTotalMatchTime - GetTotalTimePassed;
        public float GetTotalMatchTime => timeToMatchStart + matchTime + matchOverTimeDuration;
        public float GetTotalTimePassed => _totalTimePassed;

        public void AddTimedEvent(float time, Action executedFunction)
        {
            _timedMatchEvents.Add(time, executedFunction);
        }

        public bool CanRespawn()
        {
            return Mathf.Approximately(_currentTimePeriod, matchTime);
        }
    }
}