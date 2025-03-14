using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using Loading;
using MainMenu.UI;
using Managers.Local;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Utilities.General;
using Debug = UnityEngine.Debug;

namespace Managers.Network
{
    
    
    public struct BallPlayerInfo : INetworkSerializable, IEquatable<BallPlayerInfo>
    {
        public FixedString64Bytes  Username;
        public ulong ClientID;
        public float Score;
        public int TeamID;
        
        //any other data that needs to be replicated...
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Username);
                reader.ReadValueSafe(out ClientID);
                reader.ReadValueSafe(out Score);
                reader.ReadValueSafe(out TeamID);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Username);
                writer.WriteValueSafe(ClientID);
                writer.WriteValueSafe(Score);
                writer.WriteValueSafe(TeamID);
            }
        }

        public void UpdateScore(float amt)
        {
            Score += amt;
        }

        public BallPlayerInfo(FixedString64Bytes username, ulong clientID, float score, int teamID)
        {
            Username = username;
            Score = score;
            TeamID = teamID;
            ClientID = clientID;
        }

        public bool Equals(BallPlayerInfo other)
        {
            return Username == other.Username && TeamID == other.TeamID;
        }

        public override bool Equals(object obj)
        {
            return obj is BallPlayerInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Username, TeamID);
        }
    }
    
    [DefaultExecutionOrder(-500)]
    
    public class NetworkGameManager: NetworkBehaviour
    {
         [SerializeField, Min(0)] private float matchTime = 300; 
         [SerializeField, Min(0)] private float timeToMatchStart = 15; 
         [SerializeField,  Min(0)] private float matchOverTimeDuration = 120;
    
         public static NetworkGameManager Instance { get; private set; }
    
         private readonly HashSet<ulong> _players = new();


         public event Action OnHostDisconnected;
         public event Action OnAllPlayersJoined;
         public event Action<ulong, int> OnPlayerScoreUpdated;

         public event Action OnGameBegin;
         public event Action OnGameEnd;
    

         private CancellationTokenSource _matchCancelTokenSource;

         private readonly TupleList<float, Action> _timedMatchEvents = new();

         public NetworkVariable<bool> GameStarted { get; private set; } = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
         public NetworkVariable<float> CurrentTime { get; private set; } = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
         
         //doubt these two need to be replicated buttt ill make them just to be sure :P
         public NetworkVariable<float> CurrentTimePeriod { get; private set; } = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
         public NetworkVariable<float> TotalTimePassed { get; private set; } = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
         

        // public NetworkList<BallPlayerInfo> Players { get; private set; }; // = new NetworkVariable<List<BallPlayerInfo>>(new List<BallPlayerInfo>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        //the REPLICATED values every client should know about: username, score, teamid, etc...
        public NetworkList<BallPlayerInfo> Players { get; private set; } = new NetworkList<BallPlayerInfo>();
        

        private void Awake() 
        {
             //every client should make their own instance
             if (Instance != null && Instance != this)
             {
                 Destroy(gameObject);
                 return;
             }
             Instance = this; 
        }

         public override void OnNetworkSpawn()
         {
             base.OnNetworkSpawn();

             GameStarted.OnValueChanged += OnGameStarted_Multicast;
             CurrentTime.OnValueChanged += OnCurrentTime_Multicast;
             Players.OnListChanged += PlayersOnOnListChanged;
             
             CheckGameStart_ServerRpc(PlayerBallInfo.UserName);
         }


         private void PlayersOnOnListChanged(NetworkListEvent<BallPlayerInfo> changeevent)
         {
             if (Players.Count == NetworkManager.ConnectedClients.Count)
             {
                 OnAllPlayersJoined?.Invoke();
             }
         }


         private void OnEnable()
         {
             if (!NetworkManager)
             {
                 Debug.LogWarning("No Network Manager, Playing offline");
                 return;
             }

             NetworkManager.OnClientConnectedCallback += OnClientConnected;
             NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
             NetworkManager.OnServerStopped += OnServerStopped;
         }
         private void OnDisable()
         {
             if (!NetworkManager)
             {
                 Debug.LogWarning("No Network Manager, Playing offline");
                 return;
             }
             
             NetworkManager.OnClientConnectedCallback -= OnClientConnected;
             NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
             NetworkManager.OnServerStopped -= OnServerStopped;
         }
    
         void OnClientConnected(ulong clientId)
         {
             _players.Add(clientId);
        
             CheckStartGame();
         }
    
         void OnClientDisconnected(ulong clientId)
         {
             Debug.Log("client disconnected");
             foreach (BallPlayerInfo player in Players)
             {
                 if (clientId == player.ClientID)
                 {
                     Players.Remove(player);
                     Debug.Log("removing player");
                 }
             }
             _players.Remove(clientId);
             
         }

         [ServerRpc(RequireOwnership = false)]
         private void CheckGameStart_ServerRpc(FixedString64Bytes playerName, ServerRpcParams @params = default)
         {
             Debug.Log("Player has connected and been registered: " + @params.Receive.SenderClientId);

             //server only list...
             _players.Add(@params.Receive.SenderClientId);
             Players.Add(new BallPlayerInfo(playerName, @params.Receive.SenderClientId,0, 0));
             
             CheckStartGame();
         }

         public void OnPlayerKilled(ulong killedID, ulong killerID)
         {
             int index = GetPlayerBallInfoIndex(killerID);

             if (index != -1)
             {
                 Players[index].UpdateScore(1);
                 OnPlayerScoreUpdated_ClientRpc(killerID);
             }
             
             ClientRpcParams rpcParams = default;
             rpcParams.Send.TargetClientIds = new[] { killerID };
             OnLocalPlayerKilledPlayer_ClientRpc(killedID, rpcParams);
         }

         [ClientRpc(RequireOwnership = false)]
         void OnLocalPlayerKilledPlayer_ClientRpc(ulong killedID, ClientRpcParams rpcParams = default)
         {
             GameUI.Instance.ShowElimUI(GetPlayerName(killedID));
         }

         [ClientRpc(RequireOwnership = false)]
         void OnPlayerScoreUpdated_ClientRpc(ulong clientID)
         {
             OnPlayerScoreUpdated?.Invoke(clientID, 1);

         }
        
         private void OnServerStopped(bool obj)
         {
             Debug.LogError("Server stopped IMPLEMENT RETURN TO MENU OR HOST MIGRATION.");
             OnHostDisconnected?.Invoke();
         }

         private void CheckStartGame()
         {
             if (_players.Count == NetworkManager.ConnectedClients.Count)
             {
                 GameStarted.Value = true;
            
                 //only the server should be updating the match timers... clients can get that info through the NetworkedVariables, this is to keep it consistent 
                 _matchCancelTokenSource =  new CancellationTokenSource();
                 _ = ManagerMatchTimer(_matchCancelTokenSource.Token);
             }
         }

         //called when the gamestarted variable has replicated to the client....
         private void StartGame_Client()
         {
             Debug.LogWarning("Client knows the game is starting...");
             LoadingHelper.Instance.Deactivate();

         }

         private void OnGameStarted_Multicast(bool old, bool current)
         {
             if (current)
             {
                 StartGame_Client();
                 OnGameBegin?.Invoke();
             }
             else  if(old)
             {
                 OnGameEnd?.Invoke();
             }
         }

         private void OnCurrentTime_Multicast(float old, float current)
         {
        
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
             GameStarted.Value = false;
         }
    
         private async UniTask ProcessTimeFrame(float duration, CancellationToken token)
         {
             CurrentTimePeriod.Value = duration;
             while (CurrentTime.Value < CurrentTimePeriod.Value)
             {
                 float dt = Time.deltaTime;
                 CurrentTime.Value += dt;
                 TotalTimePassed.Value += dt;
                // Debug.LogWarning("time event balls" + _timedMatchEvents.Count);

                 if (_timedMatchEvents.Count != 0 && GetTotalTimePassed >= _timedMatchEvents[0].Item1)
                 {
                     Debug.Log("Executing a timed event at time: " + GetTotalTimePassed);
                     _timedMatchEvents.Remove(_timedMatchEvents[0]);
                     _timedMatchEvents[0].Item2.Invoke();
                 }
                 await UniTask.Yield(token);
             }
             CurrentTime.Value = 0;
         }

         public float EvaluateCurrentTimePeriodAsPercent => CurrentTime.Value / CurrentTimePeriod.Value;
         public float GetRemainingSectionTime => CurrentTimePeriod.Value - CurrentTime.Value;
         public float GetRemainingTime => GetTotalMatchTime - GetTotalTimePassed;
         public float GetTotalMatchTime => timeToMatchStart + matchTime + matchOverTimeDuration;
         public float GetTotalTimePassed => TotalTimePassed.Value;
    
         public void AddTimedEvent(float time, Action executedFunction)
         {
             _timedMatchEvents.Add(time, executedFunction);
             _timedMatchEvents.Sort();
         }

         public bool CanRespawn()
         {
             return Mathf.Approximately(CurrentTimePeriod.Value, matchTime) && GameStarted.Value;
         }


         public int GetPlayerBallInfoIndex(ulong clientID)
         {
             for (int i = 0; i < Players.Count; i++)
             {
                 if (clientID == Players[i].ClientID)
                 {
                     return i;
                 }
             }

             return -1;
         }

         public BallPlayerInfo GetLocalPlayerInfo()
         {
             foreach(var player in Players)
             {
                 if (player.ClientID == NetworkManager.LocalClientId)
                 {
                     return player;
                 }
             }

             return new BallPlayerInfo();
         }

         public List<BallPlayerInfo> GetAllPlayersExcludingLocalPlayer()
         {
             ulong localClientID = NetworkManager.LocalClientId;
             List<BallPlayerInfo> players = new List<BallPlayerInfo>();
             
             foreach(var player in Players)
             {
                 if (player.ClientID == localClientID)
                 {
                     continue;
                 }
                 
                 players.Add(player);
             }

             return players;
         }

         public BallPlayerInfo GetPlayerInfo(ulong id)
         {
             foreach(var player in Players)
             {
                 if (player.ClientID == id)
                 {
                     return player;
                 }
             }

             return new BallPlayerInfo();
         }

         public string GetPlayerName(ulong id)
         {
             foreach(var player in Players)
             {
                 if (player.ClientID == id)
                 {
                     return player.Username.ToString();
                 }
             }

             return "";
         }
         
    
         //ideally particles shouldn't be spawned with RPC'S, they should be spawned with replicated variables... atleast in unreal, not sure in this.. so leaving it as is for now.
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

         public float GetTimeLeftInMatch()
         {
             return matchTime - CurrentTime.Value;
         }
        /*/
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
        
        public NetworkVariable<bool> GameStarted { get; private set; } = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log("ON NETWORK GAME MANAGER, IS SERVER: " + IsServer);
            
            enabled = IsServer;
          
            
            CheckGameStart_ServerRpc();
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
        /*/
    }
}