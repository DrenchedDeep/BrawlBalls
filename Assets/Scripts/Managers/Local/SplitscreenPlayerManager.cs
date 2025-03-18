using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers.Local
{
    [RequireComponent(typeof(PlayerInputManager))]
    public class SplitscreenPlayerManager : MonoBehaviour
    {
        /* Do we want to destroy if we're on phone? Will we have other responsibilites?
        #if UNITY_MOBILE && !UNITY_EDITOR
        private void OnEnable()
        {   
            Destroy(gameObject);
        }
        #endif
        */
        private PlayerInputManager _playerInputManager;
        
        public static SplitscreenPlayerManager Instance { get; private set; }
        
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
            Debug.Log("A player has disconnect");
            
        }

        private void OnPlayerJoined(PlayerInput playerInput)
        {
            Debug.Log("A player has Joined");
            
            var device = playerInput.devices[0];

            if (device is Gamepad) playerInput.SwitchCurrentControlScheme("Controller", device);
            else if (device is Keyboard || device is Mouse) playerInput.SwitchCurrentControlScheme("Keyboard", playerInput.devices.ToArray());
        

            int id = (StaticUtilities.PlayerOneLayerLiteral + playerInput.playerIndex);
            
            Transform[] transforms = playerInput.GetComponentsInChildren<Transform>();
            foreach (var tr in transforms)
            {
                tr.gameObject.layer = id;
            }

            Camera cam =playerInput. GetComponentInChildren<Camera>();

            OutputChannels myChannel = (OutputChannels)(2 << playerInput.playerIndex);
            
            cam.GetComponent<CinemachineBrain>().ChannelMask =  (OutputChannels)1 | myChannel;

            int shiftedID = 1 << id;
            
            int cullingLayers = StaticUtilities.ExcludePlayers( shiftedID);
            
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
            
            //AudioListener lister = cam.GetComponent<AudioListener>();
            //Destroy(lister);
        }


        public void SetLocalMultiplayerJoinable(bool state)
        {
            if(state) _playerInputManager.EnableJoining();
            else _playerInputManager.DisableJoining();
        }
        
        
    }
}
