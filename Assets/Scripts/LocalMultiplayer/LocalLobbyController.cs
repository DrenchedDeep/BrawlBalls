using MainMenu.UI;
using Managers.Network;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace LocalMultiplayer
{
    /// <summary>
    /// This classes purpose is to bridge the gap for players to connect to the lobby from a remote instance.
    /// </summary>
    public class LocalLobbyController : MonoBehaviour
    {
        [SerializeField] private GameObject[] hostOnlyObjects;
        [SerializeField] private PlayerCard[] playerCards;
        [SerializeField] private Button beginGameButton;
        [SerializeField] private Button startSearchButton;
        [SerializeField] private Button stopSearchButton;

        private LocalPlayer _localPlayer;

        private void Start()
        {
            _localPlayer = GetComponent<LocalPlayer>();
            RefreshLocal();
            SplitscreenPlayerManager.Instance.OnLocalSplitscreenHostChanged += RefreshLocal;
        }

        private void OnEnable()
        {
            LobbySystemManager.Instance.OnClientConnected += LazyRegenCards;
            LobbySystemManager.Instance.OnClientDisconnected += LazyRegenCards;
            LobbySystemManager.Instance.OnLobbyOpened += LazyRegenCards;
            LobbySystemManager.Instance.OnLobbyClosed += ClearCards;
            LobbySystemManager.Instance.OnGameStarting += DisableInput;
        }
        

        private void OnDisable()
        {
            LobbySystemManager.Instance.OnClientConnected -= LazyRegenCards;
            LobbySystemManager.Instance.OnClientDisconnected -= LazyRegenCards;
            LobbySystemManager.Instance.OnLobbyOpened -= LazyRegenCards;
            LobbySystemManager.Instance.OnLobbyClosed -= ClearCards;
            LobbySystemManager.Instance.OnGameStarting -= DisableInput;
            if(SplitscreenPlayerManager.Instance) SplitscreenPlayerManager.Instance.OnLocalSplitscreenHostChanged -= RefreshLocal;
        }
        
        private void RefreshLocal()
        {
            if(LobbySystemManager.Instance.MyLobby != null)
                LobbySystemManager.Instance.LeaveLobby();

            bool x = IsCouchCoopHost();
            startSearchButton.gameObject.SetActive(x);
            beginGameButton.gameObject.SetActive(false);
            stopSearchButton.gameObject.SetActive(false);
            
            if (x)
            {
                beginGameButton.onClick.AddListener(LobbySystemManager.Instance.StartGame);
                startSearchButton.onClick.AddListener(LobbySystemManager.Instance.QuickPlay);
                stopSearchButton.onClick.AddListener(LobbySystemManager.Instance.LeaveLobby);
            }

            foreach (var obj in hostOnlyObjects)
            {
                obj.SetActive(x);
            }
            
            LazyRegenCards();
        }


        private void DisableInput()
        {
            Debug.Log("We are disabling input...");
            Debug.LogWarning("There's no active handling for if a player disconnects during this state.");
            beginGameButton.interactable = false;
            startSearchButton.gameObject.SetActive(false);
            stopSearchButton.gameObject.SetActive(false);
        }
        
        private void ClearCards()
        {
            Debug.Log("We are clearing every card!");
            foreach (var t in playerCards)
            {
                t.RemovePlayer();
            }
            
            
        }


        private void LazyRegenCards()
        {
            Debug.Log("Lazily regenerating my local players cards: ", _localPlayer);

            Lobby lobby = LobbySystemManager.Instance.MyLobby;

            
            bool isHost = IsLobbyHost() && IsCouchCoopHost();
            
            Debug.Log("Is local player host? " + LobbySystemManager.Instance.IsHost() +" && " + IsCouchCoopHost());
            
            beginGameButton.gameObject.SetActive(isHost);
            beginGameButton.interactable = isHost;

            if (lobby == null) return;
            
            string id = _localPlayer.name;

            //Iterate through all the cards we own
            int playerIndex = 0;
            for (int index = 0; index < playerCards.Length; index++)
            {
                
                if (playerIndex >= lobby.Players.Count)
                {
                    playerCards[index].RemovePlayer();
                    continue;
                }
                
                Player player = lobby.Players[playerIndex++];
                
                int numChildren = int.Parse(player.Data["ChildCount"].Value);

                string userName = player.Data["Name"].Value;
                playerCards[index].UpdatePlayer( userName, "1",  userName == id, false);
                
                //Go through all the children, and enable a card for each child
                for (int i = 1; i <= numChildren; ++i)
                {
                    Debug.Log("Trying to read user at index: " + i);
                    if (++index >= playerCards.Length) break;
                    userName = player.Data["Name" + i].Value;
                    Debug.Log("Trying to read user at index: " + i + player.Data["Name" + i].Value);
                    playerCards[index].UpdatePlayer( userName, "1",  userName == id, true);
                }

            }

        }

        public bool IsLobbyHost() => LobbySystemManager.Instance.IsHost();
        public bool IsCouchCoopHost() => SplitscreenPlayerManager.Instance.LocalHost.playerIndex == _localPlayer.input.playerIndex;
        
        
        public void StartFindingMatch()
        {
            Debug.Log("StartFindingMatch", gameObject);
            LobbySystemManager.Instance.QuickPlay();
        }

        public void StopFindingMatch()
        {
            Debug.Log("StopFindingMatch", gameObject);
            LobbySystemManager.Instance.LeaveLobby();

        }

        public void ForceStartMatch()
        {
            Debug.Log("ForceStartMatch", gameObject);
            LobbySystemManager.Instance.StartGame();

        }
    }
}