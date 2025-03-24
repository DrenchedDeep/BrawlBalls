using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
//using Utilities.UI_General;

namespace Managers.Local
{
    [RequireComponent(typeof(PlayerInputManager)), DefaultExecutionOrder(1000)]
    public class SplitscreenPlayerManager : MonoBehaviour
    {
        private PlayerInputManager _playerInputManager;
        //private PlayerInput _firstPlayer;
        public static SplitscreenPlayerManager Instance { get; private set; }
        public int PlayerCount { get; set; }


        //private bool _firstPlayerReplaced;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            //DontDestroyOnLoad(gameObject);

            _playerInputManager = GetComponent<PlayerInputManager>();
            
            _playerInputManager.playerJoinedEvent.AddListener(OnPlayerJoined);
            _playerInputManager.playerLeftEvent.AddListener(OnPlayerLeft);
            
            
        }


        private void OnPlayerLeft(PlayerInput playerInput)
        {
            Debug.Log("A player has disconnected");
            PlayerCount -= 1;
        }

    private void OnPlayerJoined(PlayerInput playerInput)
        {
            Debug.Log("A player has Joined");
            PlayerCount += 1;
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
        }




    }
}
