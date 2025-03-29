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
        [SerializeField] private GameObject screenBlockerPrefab;

        private GameObject _screenBlocker;
        
        //[SerializeField] private InputActionReference leaveGameAction;
        private static readonly List<DataParasite> ActiveParasites = new();

        private void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneChanged;
        }

        private void Awake()
        {
       
            
            _playerInputManager = GetComponent<PlayerInputManager>();
            
           // _playerInputManager.playerPrefab = SceneManager.GetActiveScene().buildIndex == 1 ? mainMenuPrefab : inGamePrefab;
            
           // _playerInputManager.onPlayerJoined += (OnPlayerJoined);
          //  _playerInputManager.onPlayerLeft += (OnPlayerLeft);

            
            Debug.LogWarning("We're doing a temporary check to see if any objects in the scene already have playerInput. In the future, this should not be the case...");
            LocalHost = FindFirstObjectByType<PlayerInput>(FindObjectsInactive.Exclude);
            
        }

        private void Start()
        {
            LobbySystemManager.Instance.OnGameStarting += CreateDataParasites;
        }

        private void CreateDataParasites()
        {
            ActiveParasites.Clear();
            foreach (var t in LocalPlayers)
            {
                ActiveParasites.Add(new DataParasite(t, t.playerIndex, t.splitScreenIndex));
                Debug.Log("Storing player index: " + t.playerIndex +", " + t.splitScreenIndex);
            }
            Debug.Log("Player Data Containers have been generated.");
        }


        private void OnSceneChanged(Scene newScene, LoadSceneMode loadSceneMode)
        {          
            Debug.Log("We are now in a new scene: " + newScene.name);
            _playerInputManager.playerPrefab =newScene.buildIndex < 2 ? mainMenuPrefab : inGamePrefab;
            LocalPlayers.Clear();
            SaveManager.Clear();
            if(newScene.buildIndex == 1) _playerInputManager.EnableJoining();
            #if !UNITY_EDITOR
            else {
_playerInputManager.DisableJoining();
Debug.Log("We've disabled controls because you may not join from this scene.")
            }
#endif
            Debug.Log("We've loaded a scene, do we know about anyone that currently exists?" + LocalPlayers.Count +"...Data containers: " + (ActiveParasites?.Count ?? 0) +$"Build index: {newScene.buildIndex}");

            if (newScene.buildIndex == 0) return;
            DeployParasites();


        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneChanged;
        }

        private void DeployParasites()
        {
            if (ActiveParasites == null) return;
            foreach (DataParasite player in ActiveParasites)
            {
                List<InputDevice> devices = new();
                
                foreach (var x in player.Devices)
                {
                    //InputSystem.DisableDevice(x);
                    if (!x.native) {
                        InputSystem.RemoveDevice(x);
                        continue; // Skip virtual mice
                    }
                    
                    Debug.Log($"Remember Device: {x} with user {player.PlayerIndex} at screen {player.SplitScreenIndex}");
                    devices.Add(x);
                }
                OnPlayerJoined(PlayerInput.Instantiate(_playerInputManager.playerPrefab, 
                    playerIndex: player.PlayerIndex, 
                    pairWithDevices: devices.ToArray()));
            }
            ActiveParasites.Clear();
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

            if (LocalPlayers.Count == 3)
            {
                Debug.Log("I am creating the screen blocker.");
                _screenBlocker = Instantiate(screenBlockerPrefab);
            }
            else if (_screenBlocker)
            {
                Debug.Log("I am Destroying the screen blocker.");
                Destroy(_screenBlocker.gameObject);
            }
            
            if (!LocalHost)
            {
                LocalHost = playerInput;
                Debug.Log("We have a new host: ", LocalHost.gameObject);
            }

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
                if (c.TryGetComponent(out CinemachineInputAxisController x))
                {
                    x.PlayerIndex = playerInput.playerIndex;
                }
            }
           
            OnClientsUpdated?.Invoke();
        }
    
    }

    public struct DataParasite
    {
        public readonly InputDevice[] Devices;
        public readonly int PlayerIndex;
        public readonly int SplitScreenIndex;
        public DataParasite(PlayerInput playerInput, int index, int splitScreenIndex)
        {
            _ = SaveManager.TryGetPlayerData(playerInput, out _);
            PlayerIndex = index;//output.PlayerIndex;
            SplitScreenIndex = splitScreenIndex;
            Devices = playerInput.devices.ToArray();
        }
    }
}
