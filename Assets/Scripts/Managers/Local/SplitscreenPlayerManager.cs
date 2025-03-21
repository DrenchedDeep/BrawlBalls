using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers.Local
{
    [RequireComponent(typeof(PlayerInputManager)), DefaultExecutionOrder(1000)]
    public class SplitscreenPlayerManager : MonoBehaviour
    {
        private PlayerInputManager _playerInputManager;
        private PlayerInput firstPlayer;
        public static SplitscreenPlayerManager Instance { get; private set; }

        private bool _firstPlayerReplaced;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _playerInputManager = GetComponent<PlayerInputManager>();
            
            _playerInputManager.playerJoinedEvent.AddListener(OnPlayerJoined);
            _playerInputManager.playerLeftEvent.AddListener(OnPlayerLeft);
            
            
        }


        private void OnPlayerLeft(PlayerInput playerInput)
        {
            Debug.Log("A player has disconnected");
        }

        private void OnPlayerJoined(PlayerInput playerInput)
        {
            Debug.Log("A player has Joined");

            if (!firstPlayer)
            {
                firstPlayer = playerInput;
            }
            else if (!_firstPlayerReplaced)
            {
                _firstPlayerReplaced = true;
                List<InputDevice> t = firstPlayer.devices.ToList();
                t.AddRange(playerInput.devices);
                
                firstPlayer.SwitchCurrentControlScheme("Controller",playerInput.devices.ToArray());
                Destroy(playerInput.gameObject);
                return;
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
            }
        }

    }
}
