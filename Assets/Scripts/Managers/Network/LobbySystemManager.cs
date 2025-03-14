using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Loading;
using MainMenu.UI;
using Managers.Local;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Managers.Network
{
    public class LobbySystemManager : MonoBehaviour
    {

        [SerializeField] private Button beginGameButton;
        [SerializeField] private PlayerCard[] playerCards;

        
        private Lobby _myLobby;
        private const int HeartbeatTimer = 15000;
        private const int PollTimer = 1100;
        private Player _playerObject;

        public UnityEvent onLocalDisconnectedFromLobby;
        public UnityEvent onLocalGameStarting;

        [SerializeField] private string[] inRotationMaps =
        {
            "RooftopWreckers_NEW"
        };

        private readonly LobbyEventCallbacks _events = new();

        bool isCreated = false;
        
        public void Initialize()
        {
            if (isCreated)
                return;

            isCreated = true;
            Dictionary<string, PlayerDataObject> d = new Dictionary<string, PlayerDataObject>()
            {
                //Member is visible for everyone in lobby.
                //Private is visible to self
                //Public is visible to everyone
                { "Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, SaveManager.MyBalls.Username) },
                { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0") }
            };
            
            _playerObject = new(AuthenticationService.Instance.PlayerId)
            {
                Data = d,
                
            };
            _events.DataChanged += CheckStartGame;
            //_events.PlayerJoined += x => LazyRegenCards();
            //_events.PlayerLeft += x => LazyRegenCards();
            _events.LobbyChanged += changes =>
            {
                if(changes.LobbyDeleted) ClearCards();
                else if (changes.PlayerJoined.Changed || changes.PlayerLeft.Changed)  LazyRegenCards();
            };
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
                    await UniTask.Delay(HeartbeatTimer, cancellationToken: token);  // Delay with cancellation token
                    if (_myLobby == null || token.IsCancellationRequested) return;

                    await LobbyService.Instance.SendHeartbeatPingAsync(_myLobby.Id);

                    if (token.IsCancellationRequested) return;

                    // You can decide whether to recursively call HeartBeat() or use a loop
                }
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation if necessary
            }
            
        }
        
        
        private void LazyRegenCards()
        {
            //Don't destroy cards.
            for (int index = 0; index < playerCards.Length; index++)
            {
                if (index >= _myLobby.Players.Count)
                {
                    playerCards[index].RemovePlayer();
                    continue;
                }

                Player p = _myLobby.Players[index];
                playerCards[index].UpdatePlayer(p.Data["Name"].Value, "1", p.Id == AuthenticationService.Instance.PlayerId);
            
            }

            beginGameButton.gameObject.SetActive(_myLobby.HostId == AuthenticationService.Instance.PlayerId);
        }

        private void ClearCards()
        {
            foreach (var t in playerCards)
            {
                t.RemovePlayer();
            }

            beginGameButton.gameObject.SetActive(false);
        }
        
        private async UniTask WaitForAllClientsToConnect()
        {
            int expectedClients = _myLobby.Players.Count;

            int attempts = 20; // Number of retries before timing out
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

            Debug.LogWarning($"Not all clients connected in time. {NetworkManager.Singleton.ConnectedClientsIds.Count}/{_myLobby.Players.Count}");
        }
        
        private async void CheckStartGame(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> obj)
        {
            if (_myLobby.Data["RelayCode"].Value != "0")
            {
                Debug.Log("HEARD: Game starting request: " + NetworkManager.ServerClientId + " --> " + _myLobby.Data["RelayCode"].Value);
                LoadingHelper.Instance.Activate();
                onLocalGameStarting?.Invoke();
                await RelayHandler.Instance.JoinRelay(_myLobby.Data["RelayCode"].Value);
                _myLobby = null;
            }
        }
        #region Buttons
        public async void StartGame()
        {
            if (_myLobby.HostId != AuthenticationService.Instance.PlayerId) return;
            
            LoadingHelper.Instance.Activate();

            
            string map = inRotationMaps[Random.Range(0, inRotationMaps.Length)];
            
            beginGameButton.interactable = false;

            Debug.Log("Starting game!");

            
            string relayCode = await RelayHandler.Instance.CreateRelay(_myLobby.Players.Count);

            _myLobby = await LobbyService.Instance.UpdateLobbyAsync(_myLobby.Id, new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"Map", new DataObject(DataObject.VisibilityOptions.Member, map)},
                    {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode)},
                },
                IsLocked = true,
            });

            
            onLocalGameStarting?.Invoke();
            await WaitForAllClientsToConnect();
            //await LobbyService.Instance.DeleteLobbyAsync(_myLobby.Id); // Do we need to discard the lobby?
            NetworkManager.Singleton.SceneManager.LoadScene(map, LoadSceneMode.Single);
            
            beginGameButton.interactable = true;
            
        }
                
        
        public async void QuickPlay()
        {
            Initialize();
          

            try
            {


                // Quick-join a random lobby with a maximum capacity of 10 or more players.
                QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
                {
                    Filter = new List<QueryFilter>()
                    {
                        new(QueryFilter.FieldOptions.AvailableSlots, "0",
                            QueryFilter.OpOptions.GT), // Check that there are open slots.
                        new(QueryFilter.FieldOptions.IsLocked, "0",
                            QueryFilter.OpOptions.EQ) // Make sure lobby is not locked
                    },
                    Player = _playerObject // We are the local player
                };

                _myLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
                beginGameButton.gameObject.SetActive(false);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
                {
                    try
                    {
                        int n = playerCards.Length;
                        Debug.Log("There is a max of " + n + " player cards, so the lobby size is this.");
                        _myLobby = await LobbyService.Instance.CreateLobbyAsync(
                            AuthenticationService.Instance.PlayerId + "'s lobby", n, new CreateLobbyOptions()
                            {
                                IsPrivate = false,
                                Player = _playerObject,
                                Data = new()
                                {
                                    { "Map", new DataObject(DataObject.VisibilityOptions.Member, "") },
                                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "0") }
                                }
                            });

                    }
                    catch (LobbyServiceException e2)
                    {
                        Debug.LogError("Failed to create lobby: " + e2.Reason);
                    }
                }
                else
                {
                    Debug.LogError("Failed to quick join: " + e.Reason);
                    SceneManager.LoadSceneAsync(0);
                    return;
                }
            }
            

            await LobbyService.Instance.SubscribeToLobbyEventsAsync(_myLobby!.Id, _events);

            HeartBeat();
            LazyRegenCards();

        }
        
        public async void LeaveLobby()
        {
            try
            {
                if (_myLobby == null) return;
                _cancellationTokenSource?.Cancel();
                if (_myLobby.HostId == AuthenticationService.Instance.PlayerId) // player ID is always null?
                {
                    if (_myLobby.Players.Count == 1)
                    {
                        Debug.Log("Destroying an empty lobby.");
                        await LobbyService.Instance.DeleteLobbyAsync(_myLobby.Id);
                        onLocalDisconnectedFromLobby?.Invoke();
                        ClearCards();
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
                

                await LobbyService.Instance.RemovePlayerAsync(_myLobby.Id, AuthenticationService.Instance.PlayerId);

                _myLobby = null;
                
                
                Debug.Log("I have left the lobby, later nerds!");
                
                onLocalDisconnectedFromLobby?.Invoke();
                
                ClearCards();

            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Error leaving lobby: " + e);
            }
        }
        

        private void OnEnable()
        {
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
