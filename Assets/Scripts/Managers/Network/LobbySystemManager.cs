using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Loading;
using LocalMultiplayer;
using Managers.Local;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Managers.Network
{
    [DefaultExecutionOrder(-1000)]
    public class LobbySystemManager : MonoBehaviour
    {



        public Lobby MyLobby { get; private set; }
        private const int HeartbeatTimer = 15000;
        private const int PollTimer = 1100;
        private Player _playerObjects;

        private const int MaxLobbySize = 8;
        
        public event Action OnClientConnected;
        public event Action OnClientDisconnected;
        public event Action OnLobbyClosed;
        public event Action OnLobbyOpened;
        public event Action OnGameStarting;


        [SerializeField] private string[] inRotationMaps =
        {
            "RooftopWreckers_NEW"
        };

        private readonly LobbyEventCallbacks _events = new();


        public static LobbySystemManager Instance { get; private set; }



        
        private async void RecompileLobbyParameters()
        {
            Debug.Log("Recompiling lobby parameters... " + IsHost());
            if (!IsHost()) return;
            
            int lobbySize = MaxLobbySize;
            foreach (Player player in MyLobby.Players)
            {
                if (!int.TryParse(player.Data["ChildCount"].Value, out int numChildren))
                {
                    Debug.LogError("Somehow we're trying to create an invalid number of children???: " + player.Data["ChildCount"].Value);
                    return;
                }
                lobbySize -= numChildren;
            }
            
            Debug.Log("There is now a max lobby size of: " + lobbySize);

            try
            {
                MyLobby = await LobbyService.Instance.UpdateLobbyAsync(MyLobby.Id, new UpdateLobbyOptions()
                {
                    MaxPlayers = lobbySize,
                });
            }
            catch (Exception e)
            {
                Debug.LogError("We failed to changed the lobby size: "+ e);
            }

        }


        private CancellationTokenSource _cancellationTokenSource;

        private async void HeartBeat()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            try
            {
                while (true)
                {
                    await UniTask.Delay(HeartbeatTimer, cancellationToken: token); // Delay with cancellation token
                    if (MyLobby == null || token.IsCancellationRequested) return;

                    await LobbyService.Instance.SendHeartbeatPingAsync(MyLobby.Id);

                    if (token.IsCancellationRequested) return;

                    // You can decide whether to recursively call HeartBeat() or use a loop
                }
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation if necessary
            }

        }






        private async UniTask WaitForAllClientsToConnect()
        {
            int expectedClients = MyLobby.Players.Count;

            int attempts = 10; // Number of retries before timing out
            while (attempts-- > 0)
            {
                int connectedClients = NetworkManager.Singleton.ConnectedClientsIds.Count;
                Debug.Log($"Connected Clients: {connectedClients}/{expectedClients}");

                if (connectedClients == expectedClients)
                {
                    Debug.Log("All clients are connected!");

                    return;
                }

                await UniTask.Delay(PollTimer);
            }

            Debug.LogWarning(
                $"Not all clients connected in time. {NetworkManager.Singleton.ConnectedClientsIds.Count}/{MyLobby.Players.Count}");
        }

        private async void CheckStartGame(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> obj)
        {
            if (MyLobby.Data["RelayCode"].Value != "0")
            {
                Debug.Log("HEARD: Game starting request: " + NetworkManager.ServerClientId + " --> " +
                          MyLobby.Data["RelayCode"].Value);
                LoadingHelper.Instance.Activate();
                await RelayHandler.Instance.JoinRelay(MyLobby.Data["RelayCode"].Value);
                OnGameStarting?.Invoke();
                MyLobby = null;
            }
        }

        #region Buttons

        public async void StartGame()
        {
            if (!IsHost()) return;
            
            try
            {
                LoadingHelper.Instance.Activate();


                string map = inRotationMaps[Random.Range(0, inRotationMaps.Length)];
                
                Debug.Log("Starting game!");


                string relayCode = await RelayHandler.Instance.CreateRelay(MyLobby.Players.Count);

                MyLobby = await LobbyService.Instance.UpdateLobbyAsync(MyLobby.Id, new UpdateLobbyOptions()
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "Map", new DataObject(DataObject.VisibilityOptions.Member, map) },
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
                    },
                    IsLocked = true,
                });


                
                
                
                await WaitForAllClientsToConnect();
                OnGameStarting?.Invoke();

                
                Debug.Log("Beginning Network loading!");
                //NOTE: there's a return type here that may be useful
                NetworkManager.Singleton.SceneManager.LoadScene(map, LoadSceneMode.Single);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to start the game: " + e);
                LoadingHelper.Instance.Deactivate();

            }

        }

        public bool IsHost() => MyLobby != null && MyLobby.HostId == AuthenticationService.Instance.PlayerId;

        private void PopulateLocalLobby()
        {
            int n = SplitscreenPlayerManager.Instance.LocalPlayers.Count;

            if (!SaveManager.TryGetPlayerData(SplitscreenPlayerManager.Instance.LocalPlayers[0],
                    out SaveManager.PlayerData localHost))
            {
                Debug.LogError("The local host is not present??");
                return;
            }

            Dictionary<string, PlayerDataObject> d = new Dictionary<string, PlayerDataObject>()
            {
                {
                    "Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, localHost.Username)
                },
                {
                    "ChildCount",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, (n - 1).ToString())
                },
                {
                    "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0")
                }
            };

            Debug.Log("Commiting player to lobby as LOCAL HOST: " + localHost.Username);

            for (var index = 1; index < SplitscreenPlayerManager.Instance.LocalPlayers.Count; index++)
            {
                var t = SplitscreenPlayerManager.Instance.LocalPlayers[index];
                if (!SaveManager.TryGetPlayerData(t, out SaveManager.PlayerData pd))
                {
                    Debug.LogError(
                        "Lobby is trying to initialize, but there are no players matching the id " +
                        pd.Username, t);
                    return;
                }

                d.Add("Name" + index, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, pd.Username));

                Debug.Log("Commiting player to lobby as CHILD: " + pd.Username);
            }

            _playerObjects = new Player()
            {
                Data = d
            };
        }


        public async void QuickPlay()
        {
            Debug.Log("Initializing Quick play.");

            await SaveManager.SaveAllPlayers();
            
            PopulateLocalLobby();

            int numberClients = SplitscreenPlayerManager.Instance.LocalPlayers.Count;
            try
            {
                // Quick-join a random lobby with a maximum capacity of 10 or more players.
                QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
                {
                    Filter = new List<QueryFilter>()
                    {
                        new(QueryFilter.FieldOptions.AvailableSlots, (numberClients - 1).ToString(),
                            QueryFilter.OpOptions.GT), // Check that there are open slots.
                        new(QueryFilter.FieldOptions.IsLocked, "0",
                            QueryFilter.OpOptions.EQ) // Make sure lobby is not locked
                    },
                    Player = _playerObjects // We are the local player
                };
                MyLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
                Debug.Log("We have completed joined a lobby!");
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
                {
                    bool result = await CreateLobby();

                    if (!result)
                    {
                        Debug.LogError("Failed to quick join: " + e.Reason);
                        await SceneManager.LoadSceneAsync(0);
                        return;
                    }
                }
                else
                {
                    Debug.LogError("Failed to quick join: " + e.Reason);
                    await SceneManager.LoadSceneAsync(0);
                    return;
                }
            }

            //Subscribe to that lobbies events.
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(MyLobby.Id, _events);

            //Register the heartbeat clock.
            HeartBeat();

            OnLobbyOpened?.Invoke();
        }

        private async UniTask<bool> CreateLobby()
        {
            Debug.Log("Trying to create a fresh lobby");
            try
            {
                //8 Seats total, we restrict seats belonging to our children, excluding our selves. 4 players, but 3 children.
                int childCount = SplitscreenPlayerManager.Instance.LocalPlayers.Count - 1;
                int n = MaxLobbySize - childCount;
                
                Debug.Log("This lobby will be locked to " + n + " players as there is a child count of: " + (childCount));
                MyLobby = await LobbyService.Instance.CreateLobbyAsync(
                    AuthenticationService.Instance.PlayerId + "'s lobby", n, new CreateLobbyOptions()
                    {
                        IsPrivate = false,
                        Player = _playerObjects,
                        Data = new()
                        {
                            { "Map", new DataObject(DataObject.VisibilityOptions.Member, "") },
                            { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "0") }
                        }
                    });
                Debug.Log("We successfully created the lobby");
                return true;
            }
            catch (LobbyServiceException e2)
            {
                Debug.LogError("Failed to create lobby: " + e2.Reason);
            }

            return false;
        }

        public async void LeaveLobby()
        {

            try
            {
                if (MyLobby == null) return;
                _cancellationTokenSource?.Cancel();
                if (IsHost()) // player ID is always null?
                {
                    if (MyLobby.Players.Count == 1)
                    {
                        Debug.Log("Destroying an empty lobby.");
                        await LobbyService.Instance.DeleteLobbyAsync(MyLobby.Id);
                        OnLobbyClosed?.Invoke();
                        return;
                    }
                    /*
                    await LobbyService.Instance.UpdateLobbyAsync(_myLobby.Id, new UpdateLobbyOptions()
                    {
                        HostId = _myLobby.Players[1].Id
                    });

                    Debug.Log("Host migrated to: " + _myLobby.Players[1].Id);
                    */
                    
                }
                

                await LobbyService.Instance.RemovePlayerAsync(MyLobby.Id, AuthenticationService.Instance.PlayerId);

                MyLobby = null;
                
                
                Debug.Log("I have left the lobby, later nerds!");
                
                OnLobbyClosed?.Invoke();

            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Error leaving lobby: " + e);
            }
        }
        

        private void OnEnable()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            _events.DataChanged += CheckStartGame;
            //_events.PlayerJoined += x => LazyRegenCards();
            //_events.PlayerLeft += x => LazyRegenCards();
            _events.LobbyChanged += changes =>
            {
                if (changes.LobbyDeleted)
                {
                    OnLobbyClosed?.Invoke();
                }
                else if (changes.PlayerJoined.Changed)
                {
                    OnClientConnected?.Invoke();
                }
                else if (changes.PlayerLeft.Changed)
                {
                    OnClientDisconnected?.Invoke();
                }
            };

            OnClientConnected += RecompileLobbyParameters;
            
            Application.quitting += LeaveLobby;
        }


        private void OnDisable()
        {
            Application.quitting -= LeaveLobby;
        }

        private void OnDestroy()
        {
            Application.quitting -= LeaveLobby;
        }
        #endregion
    }
}
