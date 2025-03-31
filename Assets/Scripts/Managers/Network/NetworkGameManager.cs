using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.UI;
using Loading;
using LocalMultiplayer;
using MainMenu.UI;
using Managers.Local;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.General;
using Debug = UnityEngine.Debug;

namespace Managers.Network
{
    public enum GameState
    {
        None,
        WaitingForPlayers, // WAITING FOR PLAYERS TO LOAD THE LEVEL (LOADING SCREEN SHOWN HERE)
        IntroCinematic, // EVERYBODY HAS LOADED, CLOSE LOADING SCREEN AND PLAY INTRO CINEMATIC
        SelectingBalls,// ONCE EVERY CLIENT HAS CONFIRMED THEY'VE FINISHED THE CUTSCENE, THEY CAN SELECT A BALL
        StartingGame, // ONCE EVERY CLIENT HAS SELECTED A BALL AND WAITING AT THEIR SPAWNPOINT, DO THE COUNTDOWN (3,2,1, GO!)
        InGame, // IN GAME... UPDATE TIMERS N STUFF
        EndingGame, // ENDING THE GAME... STOP PLAYER MOVEMENT, SHOW UI FOR ENDING GAME
        EndingGameCinematic, //SHOW TOP 3 PLAYERS, ANY OTHER INFO...
        KickingPlayers // SERVER KICKS PLAYERS BACK TO THE MAIN MENU
    }
    
    public struct BallPlayerInfo : INetworkSerializable, IEquatable<BallPlayerInfo>
    {
        public FixedString64Bytes  Username;
        public ulong ClientID;
        public float Score;
        public int LivesLeft;
        public int TeamID;
        public int ChildID;
        
        //any other data that needs to be replicated...
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Username);
                reader.ReadValueSafe(out ClientID);
                reader.ReadValueSafe(out Score);
                reader.ReadValueSafe(out LivesLeft);
                reader.ReadValueSafe(out TeamID);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Username);
                writer.WriteValueSafe(ClientID);
                writer.WriteValueSafe(Score);
                writer.WriteValueSafe(LivesLeft);
                writer.WriteValueSafe(TeamID);
            }
        }

        public void UpdateScore(float amt) => Score += amt;

        public void UpdateLivesLeft()
        {
            LivesLeft--;
        }

        public bool IsMyPlayer(ulong clientID, int childID)
        {
            return ClientID == clientID && childID == ChildID;
        }

        public bool IsOut()
        {
            return LivesLeft <= 0;
        } 

        public BallPlayerInfo(FixedString64Bytes username, ulong clientID, float score, int teamID, int childID)
        {
            Username = username;
            Score = score;
            TeamID = teamID;
            ClientID = clientID;
            ChildID = childID;
            LivesLeft = 3;
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
         public event Action OnPlayerListUpdated;
         public event Action OnGameBegin;
         public event Action OnGameEnd;
         
         public event Action<GameState> OnGameStateUpdated;

         public event Action OnGameReachedOverTime;
         public event Action<int> OnGameCountdownDecremented;
         public event Action<float> OnCurrentTimeChanged;
    

         private CancellationTokenSource _matchCancelTokenSource;

         private readonly TupleList<float, Action> _timedMatchEvents = new();
         
         
         public NetworkVariable<float> CurrentTime { get; private set; } = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
         
         //doubt these two need to be replicated buttt ill make them just to be sure :P
         public NetworkVariable<float> CurrentTimePeriod { get; private set; } = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); 
         public NetworkVariable<float> TotalTimePassed { get; private set; } = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
         
         
         //the REPLICATED values every client should know about: username, score, teamid, etc...
         public NetworkList<BallPlayerInfo> Players { get; private set; } = new NetworkList<BallPlayerInfo>();

         public NetworkVariable<GameState> GameState { get; private set; } =
             new NetworkVariable<GameState>(Network.GameState.None);

         private int _clientsFinishedIntroCinematic;
         private int _ballsSpawned;
         private bool _isInOverTime;
         private int _cachedCountdownTime;

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

             GameState.OnValueChanged += OnGameStateChanged;
             CurrentTime.OnValueChanged += OnCurrentTimeChange;
             Players.OnListChanged += PlayersOnOnListChanged;
             
             if (IsServer)
             {
                 GameState.Value = Network.GameState.WaitingForPlayers;
             }
             CheckGameStart_ServerRpc(SaveManager.GetCompiledNames());
             
         }

         private void OnGameStateChanged(GameState old, GameState current)
         {
             OnGameStateUpdated?.Invoke(current);

             if (current != Network.GameState.WaitingForPlayers)
             {
                 LoadingHelper.Instance.Deactivate();
             }
         }

         private void OnCurrentTimeChange(float old, float current)
         {
             if (GameState.Value == Network.GameState.InGame)
             {
                 float time = matchTime - current;
                 if (time <= 60 && !_isInOverTime)
                 {
                     _isInOverTime = true;
                     OnGameReachedOverTime?.Invoke();
                 }
             }
             else if (GameState.Value == Network.GameState.StartingGame)
             {
                 int time = (int)(timeToMatchStart - current);
                 if (time != _cachedCountdownTime)
                 {
                     OnGameCountdownDecremented?.Invoke(time);
                     _cachedCountdownTime = time;
                 }
             }
             
             OnCurrentTimeChanged?.Invoke(current);
         }
         


         private void PlayersOnOnListChanged(NetworkListEvent<BallPlayerInfo> changeevent)
         {
             /*/
             if (Players.Count == NetworkManager.ConnectedClients.Count && !GameStarted.Value)
             {
                 OnAllPlayersJoined?.Invoke();
             }
             /*/
             
             OnPlayerListUpdated?.Invoke();
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
             foreach (BallPlayerInfo player in Players)
             {
                 if (clientId == player.ClientID)
                 {
                     Players.Remove(player);
                 }
             }
             _players.Remove(clientId);
             
         }

         [ServerRpc(RequireOwnership = false)]
         private void CheckGameStart_ServerRpc(FixedString512Bytes playerNames, ServerRpcParams @params = default)
         {
             Debug.Log("Player has connected and been registered: " + @params.Receive.SenderClientId);

             _players.Add(@params.Receive.SenderClientId);

             Debug.Log("Adding conjoined player object:  " + playerNames);
             string[] players = playerNames.ToString().Split(';');
             for (int i = 0; i < players.Length; i++)
             {
                 string str = players[i];
                 Debug.Log("Adding network player while considering local multiplayer: " + str);
                 Players.Add(new BallPlayerInfo(str, @params.Receive.SenderClientId, 0, 0, i));
             }

             CheckStartGame();
         }

         public void OnPlayerKilled(ulong killedID, int killedChildID, ulong killerID, int killerChildID)
         {
             //100 id is the out of bounds
             if (killerID != 99)
             {
                 for (int i = 0; i < Players.Count; i++)
                 {
                     if (Players[i].IsMyPlayer(killerID, killerChildID) && !Players[i].IsMyPlayer(killedID, killedChildID))
                     {
                         BallPlayerInfo newInfo = Players[i];
                         newInfo.UpdateScore(1);
                         Players[i] = newInfo;
                         break;
                     }
                 }
                 
                 ClientRpcParams rpcParams = default;
                 rpcParams.Send.TargetClientIds = new[] { killerID };
                 OnLocalPlayerKilledPlayer_ClientRpc(killerChildID, GetPlayerName(killedID, killedChildID), rpcParams);
             }
         }

         [ServerRpc(RequireOwnership = false)]
         public void FuckingLazyWayToDoThis_ServerRpc(ulong clientID, int childID)
         {
             int livesLostIndex = GetPlayerBallInfoIndex(clientID, childID);
             
             
             if (livesLostIndex != -1)
             {
                 BallPlayerInfo newInfo = Players[livesLostIndex];
                 newInfo.UpdateLivesLeft();
                 Players[livesLostIndex] = newInfo;
                 CheckIfCanEndGameEarly();
             }
         }

         [ClientRpc(RequireOwnership = false)]
         void OnLocalPlayerKilledPlayer_ClientRpc(int childID, FixedString64Bytes victimName, ClientRpcParams rpcParams = default)
         {
             PlayerController pc = PlayerSplitScreenManager.Instance.FindChild(childID);
             if (pc)
             {
                 if(pc.TryGetComponent(out PlayerHUD hud))
                 {
                     hud.OnKilledPlayer(victimName.ToString());
                 }
             }
         }
        
         private void OnServerStopped(bool obj)
         {
             Debug.LogError("Server stopped IMPLEMENT RETURN TO MENU OR HOST MIGRATION.");
             OnHostDisconnected?.Invoke();
         }

         private void CheckIfCanEndGameEarly()
         {
             int playersLeft = Players.Count;

             foreach (var player in Players)
             {   
                 Debug.Log(player.Username + " has " + player.LivesLeft + " lives left.");

                 if (player.LivesLeft > 0)
                 {
                     continue;
                 }

             //    Debug.Log(player.Username + " has " + player.LivesLeft + " lives left.");
                 playersLeft--;
             }

             Debug.Log("players left ALIVE: " + playersLeft);

             int minPlayers = NetworkManager.ConnectedClients.Count <= 1 ? 0 : 1;
             //only one player or less left, we can just end game now
             if (playersLeft <= 1)
             {
                 GameState.Value = Network.GameState.EndingGame;
                 _ = EndGameEarlyTask();
             }
         }

         private void CheckStartGame()
         {
             if (_players.Count == NetworkManager.ConnectedClients.Count)
             {
                 //if all players have joined, play the intro cinematic
                 GameState.Value = Network.GameState.SelectingBalls;
             }
         }

         //called when the gamestarted variable has replicated to the client....
         private void StartGame_Client()
         {
             Debug.LogWarning("Client knows the game is starting...");
             LoadingHelper.Instance.Deactivate();
             OnAllPlayersJoined?.Invoke();
         }

         private void OnGameStartedChanged(bool old, bool current)
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

         public void ReturnToMainMenu()
         {
             if (IsServer)
             {
                 foreach (var client in NetworkManager.ConnectedClients)
                 {
                     NetworkManager.DisconnectClient(client.Key);
                 }
             }

             NetworkManager.Singleton.Shutdown();
             SceneManager.LoadScene("MainMenuNEW");
         }
        
    
         private async UniTask ManagerMatchTimer(CancellationToken token)
         {
             await ProcessTimeFrame(timeToMatchStart, token);
            
             if (token.IsCancellationRequested)
             {
                 Debug.LogWarning("MATCH IS BEING CANCELLED DURING INIT");
                 return;
             }

             GameState.Value = Network.GameState.InGame;
             Debug.Log("Game is now beginning fully! Let the games begin!");
             OnGameBegin?.Invoke();
            
             await ProcessTimeFrame(matchTime, token);
            
             if (token.IsCancellationRequested)
             {
                 Debug.LogWarning("MATCH IS BEING CANCELLED DURING GAMEPLAY");
                 return;
             }
            
             Debug.LogWarning("Well I guess everyone who's alive is a winner! TODO: Actually make this work");
             GameState.Value = Network.GameState.EndingGame;
             
             await ProcessTimeFrame(matchOverTimeDuration, token);
            
             if (token.IsCancellationRequested)
             {
                 Debug.LogWarning("MATCH IS BEING CANCELLED DURING END GAME");
                 return;
             }
            
             Debug.LogWarning("ending game cinematic time now!");
             GameState.Value = Network.GameState.EndingGameCinematic;
         }

         //temp function...
         private async UniTask EndGameEarlyTask()
         {
             await UniTask.WaitForSeconds(matchOverTimeDuration);
             GameState.Value = Network.GameState.EndingGameCinematic;
         }
    
         private async UniTask ProcessTimeFrame(float duration, CancellationToken token)
         {
             CurrentTimePeriod.Value = duration;
             while (CurrentTime.Value < CurrentTimePeriod.Value)
             {
                 float dt = Time.deltaTime;
                 CurrentTime.Value += dt;
                 TotalTimePassed.Value += dt;

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
         public float GetStartingMatchTime => timeToMatchStart - CurrentTime.Value;
    
         public void AddTimedEvent(float time, Action executedFunction)
         {
             _timedMatchEvents.Add(time, executedFunction);
             _timedMatchEvents.Sort();
         }

         public bool CanRespawn()
         {
             /*/
             return Mathf.Approximately(CurrentTimePeriod.Value, matchTime) &&
                    GameState.Value >= Network.GameState.SelectingBalls;
                    /*/

             
             return GameState.Value >= Network.GameState.SelectingBalls;
         }


         public int GetPlayerBallInfoIndex(ulong clientID, int childID)
         {
             for (int i = 0; i < Players.Count; i++)
             {
                 if (Players[i].IsMyPlayer(clientID, childID))
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

         public string GetPlayerName(ulong id, int childID)
         {
             foreach(var player in Players)
             {
                 if (player.IsMyPlayer(id, childID))
                 {
                     return player.Username.ToString();
                 }
             }

             return "";
         }
         

         //probs a better way to do this :P
         [ServerRpc(RequireOwnership = false)]
         public void ClientFinishedIntroCinematic_ServerRpc()
         {
             _clientsFinishedIntroCinematic++;

             if (_clientsFinishedIntroCinematic >= Players.Count)
             {
                 GameState.Value = Network.GameState.SelectingBalls;
             }
         }

         public void OnBallSpawned()
         {
             /*/
             if (GameState.Value != Network.GameState.InGame)
             {
                 GameState.Value = Network.GameState.InGame;
             }
             /*/
             
             if (GameState.Value == Network.GameState.SelectingBalls)
             {
                 _ballsSpawned++;

                 if (_ballsSpawned >= Players.Count)
                 {
                     GameState.Value = Network.GameState.StartingGame;
                     _matchCancelTokenSource =  new CancellationTokenSource();
                     _ = ManagerMatchTimer(_matchCancelTokenSource.Token);
                 }
             }
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

  
    }
}