using System;
using System.Collections.Generic;
using Managers.Local;
using Managers.Network;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//using Utilities.UI_General;

namespace LocalMultiplayer
{
    [RequireComponent(typeof(PlayerInputManager)), DefaultExecutionOrder(-1000)]
    public class SplitscreenPlayerManager : MonoBehaviour
    {
        private PlayerInputManager _playerInputManager;
        //private PlayerInput _firstPlayer;
        public static SplitscreenPlayerManager Instance { get; private set; }
        public readonly List<PlayerInput> LocalPlayers = new();
        public PlayerInput LocalHost { get; private set; }
        public event Action OnLocalSplitscreenHostChanged;
        public event Action OnClientsUpdated;
        
        [SerializeField] private GameObject mainMenuPrefab;
        [SerializeField] private GameObject inGamePrefab;
        //[SerializeField] private InputActionReference leaveGameAction;
        private DataParasite[] _activeParasites;

        private void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Awake()
        {
       
            
            _playerInputManager = GetComponent<PlayerInputManager>();
            
           // _playerInputManager.playerPrefab = SceneManager.GetActiveScene().buildIndex == 1 ? mainMenuPrefab : inGamePrefab;
            
           // _playerInputManager.onPlayerJoined += (OnPlayerJoined);
          //  _playerInputManager.onPlayerLeft += (OnPlayerLeft);

            
            Debug.LogWarning("We're doing a temporary check to see if any objects in the scene already have playerInput. In the future, this should not be the case...");
            LocalHost = FindFirstObjectByType<PlayerInput>(FindObjectsInactive.Exclude);
            SceneManager.sceneLoaded += OnSceneChanged;
        }

        private void Start()
        {
            LobbySystemManager.Instance.OnGameStarting += CreateDataParasites;
        }

        private void CreateDataParasites()
        {
            _activeParasites = new DataParasite[LocalPlayers.Count];
            for (int i = 0; i < _activeParasites.Length; ++i)
            {
                _activeParasites[i] = new DataParasite(LocalPlayers[i]);
            }
            Debug.Log("Player Data Containers have been generated.");
        }


        private void OnSceneChanged(Scene scene, LoadSceneMode arg1)
        {          
            Debug.Log("We are now in a new scene: " + scene.name);
            _playerInputManager.playerPrefab =scene.buildIndex < 2 ? mainMenuPrefab : inGamePrefab;
            LocalPlayers.Clear();
            SaveManager.Clear();
            if(scene.buildIndex == 1) _playerInputManager.EnableJoining();
            #if !UNITY_EDITOR
            else {
_playerInputManager.DisableJoining();
Debug.Log("We've disabled controls because you may not join from this scene.")
            }
#endif
            Debug.Log("We've loaded a scene, do we know about anyone that currently exists?" + LocalPlayers.Count +"...Data containers: " + (_activeParasites?.Length ?? 0) +" Build index: " + scene.buildIndex);

            if (scene.buildIndex == 0) return;
            if (_activeParasites == null) return;
            foreach (DataParasite player in _activeParasites)
            {
                OnPlayerJoined(PlayerInput.Instantiate(_playerInputManager.playerPrefab, pairWithDevices: player.Devices));
            }

        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneChanged;
        }


        private void OnPlayerLeft(PlayerInput playerInput)
        {
            Debug.Log("A player has disconnected");
            LocalPlayers.Remove(playerInput);
            
            
            if (LocalHost == playerInput)
            {
                if(LocalPlayers.Count == 0)
                {
                    Debug.Log("Game should be shutting down");
                    return;
                }
                LocalHost = LocalPlayers[0];
                OnLocalSplitscreenHostChanged?.Invoke();
            }
            OnClientsUpdated?.Invoke();
        }

    private void OnPlayerJoined(PlayerInput playerInput)
        {
            Debug.Log("A player has Joined");

            LocalPlayers.Add(playerInput);

            
            Debug.LogError("REMINDER, Sync playerIndex? Idk too tired.");
            if (!LocalHost)
            {
                LocalHost = playerInput;
                Debug.Log("We have a new host: ", LocalHost.gameObject);
            }
            /*
            if (!_firstPlayer)
            {
                _firstPlayer = playerInput;
            }
            else if (!_firstPlayerReplaced)
            {
                _firstPlayerReplaced = true;

                Debug.Log("I've made this player in my image (the image of player 1), but now with a controller");
                _firstPlayer.SwitchCurrentControlScheme("Controller", playerInput.devices.ToArray());
                Destroy(playerInput.gameObject);

                _firstPlayer.GetComponentInChildren<BestVirtualCursor>().SetNewOwner(_firstPlayer);
                
                
                
                return;
            }
            */

            bool controller = false;
            foreach (var div in playerInput.devices)
            {
                if (div is Gamepad)
                {
                    Debug.Log("A player has joined: We'd like to choose a controller");
                    controller = true;
                    break;
                }
            }

            playerInput.SwitchCurrentControlScheme(controller ? "Controller" : "PC", playerInput.devices.ToArray());

    

            int id = (StaticUtilities.PlayerOneLayerLiteral + playerInput.playerIndex);
            
            Transform[] transforms = playerInput.GetComponentsInChildren<Transform>();
            foreach (var tr in transforms)
            {
                tr.gameObject.layer = id;
            }

            Camera cam = playerInput.GetComponentInChildren<Camera>();

            OutputChannels myChannel = (OutputChannels)(2 << playerInput.playerIndex);

            cam.GetComponent<CinemachineBrain>().ChannelMask = (OutputChannels)1 | myChannel;

            int shiftedID = 1 << id;

            int cullingLayers = StaticUtilities.ExcludePlayers(shiftedID);

            cam.cullingMask = cullingLayers;

            Light[] lights = playerInput.GetComponentsInChildren<Light>();

            foreach (var l in lights)
            {
                l.cullingMask = shiftedID;
            }

            CinemachineCamera[] cams = playerInput.GetComponentsInChildren<CinemachineCamera>();
            foreach (var c in cams)
            {
                c.OutputChannel = myChannel;
            }
            // if(controller) playerInput.GetComponentInChildren<BestVirtualCursor>().SetNewOwner(playerInput);
           
            OnClientsUpdated?.Invoke();
        }
    }

    public struct DataParasite
    {
        public readonly InputDevice[] Devices;
        public readonly int PlayerIndex;
        public DataParasite(PlayerInput playerInput)
        {
            _ = SaveManager.TryGetPlayerData(playerInput, out var output);
            PlayerIndex = output.PlayerIndex;
            Devices = playerInput.devices.ToArray();
        }
    }
}
