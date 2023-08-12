using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Player = Unity.Services.Lobbies.Models.Player;
using Random = UnityEngine.Random;

public class LobbyHandler : MonoBehaviour
{
    private Lobby myLobby;
    private const int HeartbeatTimer = 15000;
    private const int PollTimer = 1100;
    private string _nameTest = "G";

    [SerializeField] private PlayerCard[] playerCards;
    [SerializeField] private ReadyButton readyButton;
    [SerializeField] private Button startGame;
    [SerializeField] private Button startGameTemp;

    private Player _playerObject = new();
    private Player _curPlayer;
    private List<Player> _players = new();
    private static int clientsConnected;
    private static string map;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        BeginLobbySystem();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);
            QuickPlay();
        };

#if UNITY_EDITOR
        // ParrelSync should only be used within the Unity Editor so you should use the UNITY_EDITOR define
        if (ParrelSync.ClonesManager.IsClone())
        {
            // When using a ParrelSync clone, switch to a different authentication profile to force the clone
            // to sign in as a different anonymous user account.
            string customArgument = ParrelSync.ClonesManager.GetArgument();
            AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
        }
#endif
        //Access Authentication services
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void BeginLobbySystem()
    {
        _nameTest += Random.Range(0, 100);
        Dictionary<string, PlayerDataObject> d = new Dictionary<string, PlayerDataObject>()
        {
            //Member is visible for everyone in lobby.
            //Private is visible to self
            //Public is visible to everyone
            { "Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _nameTest) },
            { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0") }
        };
/*
        for (int index = 0; index < PlayerBallInfo.Instance.Balls.Length; index++)
        {
            PlayerBallInfo.BallStructure ball = PlayerBallInfo.Instance.Balls[index];
            //Is this necessary... Including it in the player data would only prevent the player from lying after the game started...
            //Should the game just trust the local player?
            d.Add(index+"Ball", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Private, ball.Ball));
            d.Add(index+"Weapon", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Private, ball.Weapon));
            d.Add(index+"Ability", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Private, ball.Ability));
        }*/


        _playerObject = new()
        {
            Data = d
        };
    }


    //This should be used only on host...?
    private async void HeartBeat()
    {
        await Task.Delay(HeartbeatTimer);
        if (myLobby == null) return;
        await LobbyService.Instance.SendHeartbeatPingAsync(myLobby.Id);
        HeartBeat();
    }

    private async void PollForUpdates()
    {
        await Task.Delay(PollTimer);
        if (myLobby == null) return;
        myLobby = await LobbyService.Instance.GetLobbyAsync(myLobby.Id);
       
        HandleChanges();


        //Game starting!
        if (myLobby.Data["RelayCode"].Value != "0")
        {
            Debug.Log("HEARD: Game starting request: " + NetworkManager.ServerClientId);
            await RelayHandler.Instance.JoinRelay(myLobby.Data["RelayCode"].Value );
            myLobby = null;
            return;
        }
        PollForUpdates();
    }

    /*
    private IEnumerator WaitForAllConnections(string map)
    {
        
        WaitForSeconds w = new WaitForSeconds(0.1f);
        while (clientsConnected > 0)
        {
            print("Remaining Connections: " + clientsConnected);
            yield return w;
        }
        print("Remaining Connections: " + clientsConnected);
        
    }*/

    public static void ConnectedToRelay()
    {
        clientsConnected--;
        //Why is this printing on the client?
        print("Total Clients Connected: " + clientsConnected);
        if (clientsConnected == 0)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(map, LoadSceneMode.Single);
        }
    }





    private void HandleChanges()
    {
        //Iterate through all the players in the lobby, check if any are different...
        int readyPlayers = 0;
        
        if (myLobby.Players.Count != _players.Count)
        {
            LazyRegenCards();
            return;
        }
        
        
        

        for (var index = 0; index < myLobby.Players.Count; index++)
        {

            Player player = myLobby.Players[index];
            
            if (_curPlayer.Id == player.Id)
            {
                _curPlayer = player;
            }
            
            Player previous = _players[index];
            if (player.Data["Name"].Value != previous.Data["Name"].Value)
            {
                //If we've gained a name mismatch, then someone joined and another left.
                LazyRegenCards();
                return;
            }
            //print("Checking ready: " + player.Data["Ready"].Value +", " + previous.Data["Ready"].Value);
            if (player.Data["Ready"].Value == "1")
                readyPlayers++;
                
            if (player.Data["Ready"].Value!= previous.Data["Ready"].Value)
            {
                print("Updating Ready on player: " + player.Data["Ready"].Value +", " + previous.Data["Ready"].Value);
                playerCards[index].UpdateReady(player.Data["Ready"].Value);
            }
        }
        _players = myLobby.Players;
        startGame.interactable = readyPlayers >= _players.Count / 2;
        startGameTemp.interactable = startGame.interactable;
    }

    

    private void LazyRegenCards()
    {
        //Don't destroy cards.
        _players = myLobby.Players;
        
        for (int index = 0; index < playerCards.Length; index++)
        {
            if (index >= _players.Count)
            {
                playerCards[index].RemovePlayer();
                continue;
            }
            Player p = _players[index];
            playerCards[index].UpdatePlayer(p.Data["Name"].Value, "1", p.Id == AuthenticationService.Instance.PlayerId);
            
        }

        startGame.gameObject.SetActive(myLobby.HostId == AuthenticationService.Instance.PlayerId);
        startGameTemp.gameObject.SetActive(myLobby.HostId == AuthenticationService.Instance.PlayerId);
        
        
        OnLobbyUpdate();
    }
    
    public async void ToggleReadyStatus()
    {
        string s = _curPlayer.Data["Ready"].Value=="0"?"1":"0";
        //print();
        print(s);
        myLobby = await LobbyService.Instance.UpdatePlayerAsync(myLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions()
        {
            Data = new Dictionary<string, PlayerDataObject>()
            {
                { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, s) }
            }
        });
                

        HandleChanges();
        
        print(_curPlayer.Data["Ready"].Value +" , " + AuthenticationService.Instance.PlayerId +" , " + _curPlayer.Id);

        readyButton.StartDelay();
    }

    private async void OnLobbyUpdate()
    {
        myLobby = await LobbyService.Instance.UpdatePlayerAsync(myLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions()
        {
            Data = new Dictionary<string, PlayerDataObject>()
            {
                { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0") }
            }
        });
        HandleChanges();
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(myLobby.Id, AuthenticationService.Instance.PlayerId);
            myLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Error leaving lobby: " + e);
        }
    }
    public async void QuickPlay()
    {
        try
        {
            // Quick-join a random lobby with a maximum capacity of 10 or more players.
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Filter = new List<QueryFilter>()
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots,
                        op: QueryFilter.OpOptions.GT,
                        value: "0")
                },
                Player = _playerObject
            };
            GameManager.IsOnline = true;
            myLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            
            //Only hosts can do this
            //TODO myLobby = await LobbyService.Instance.UpdateLobbyAsync(myLobby.Id, new ());
        }
        catch (LobbyServiceException e)
        {
            //Debug.Log(e);

            if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
            {
                myLobby = await LobbyService.Instance.CreateLobbyAsync(AuthenticationService.Instance.PlayerId+"'s lobby", 8, new CreateLobbyOptions()
                {
                   IsPrivate = false,
                   Player = _playerObject,
                   Data = new()
                   {
                       {"Map", new DataObject(DataObject.VisibilityOptions.Member, "Test")},
                       {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member, "0")}
                   }
                });
                    //myLobby.LobbyCode
                    GameManager.IsOnline = true;

                HeartBeat();
            }
            else
            {
                GameManager.IsOnline = false;
                print("Failed to connect to internet...");
                return;
            }
            HandleChanges();
        }

        _curPlayer = myLobby.Players[^1];
        PollForUpdates();
        print("Lobby succeeded? " + myLobby.Name);
    }


    public async void StartGame(string m)
    {

        if (!GameManager.IsOnline)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(m, LoadSceneMode.Single);
            return;            
        }

        if (myLobby.HostId != AuthenticationService.Instance.PlayerId) return;

        startGameTemp.interactable = false;
        startGame.interactable = false;
        
        Debug.Log("Starting game!");
        
        string relayCode = await RelayHandler.Instance.CreateRelay(myLobby.MaxPlayers);
        
        myLobby = await LobbyService.Instance.UpdateLobbyAsync(myLobby.Id, new UpdateLobbyOptions()
        {
            Data = new Dictionary<string, DataObject>
            {
                {"Map", new DataObject(DataObject.VisibilityOptions.Member, m)},
                {"RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode)},
            },
            IsLocked = true
        });
        clientsConnected = myLobby.Players.Count-1;
        map = myLobby.Data["Map"].Value;
        myLobby = null;
        //StartCoroutine(WaitForAllConnections(map));
        

    }

}
