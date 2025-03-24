using System;
using Core.ActionMaps;
using MainMenu.UI;
using Managers.Local;
using Managers.Network;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MainMenu
{
    [RequireComponent(typeof(PlayerInput))]
    public class MainMenuPlayer : MonoBehaviour
    {
        PlayerInput _input;
        

        public event Action<InputSpriteActionMap> OnDeviceChanged;
        public InputSpriteActionMap CurrentSpriteActionMap { get; private set; }
        
        public PlayerCard[] playerCards;
        public Button beginGameButton;


        private void Start()
        {
            _input = GetComponent<PlayerInput>();
            //CurrentSpriteActionMap = ResourceManager.Instance.GetActionMap(_input.devices);
            //OnDeviceChanged?.Invoke(CurrentSpriteActionMap);
            
            //_input.onDeviceRegained += OnChangedDevice;
           // _input.onDeviceLost += OnChangedDevice;
            
            //Okay so, what we want to do is make the game entirely controllable via buttons while controller is connected. We need to make selectable objects more clear.
            //Additionally, we want to add hotkeys to do some actions quicker.

        }

        private void OnChangedDevice(PlayerInput obj)
        {
            CurrentSpriteActionMap = ResourceManager.Instance.GetActionMap(obj.devices);
            OnDeviceChanged?.Invoke(CurrentSpriteActionMap);
        }

        public void StartFindingMatch()
        {
            Debug.Log("StartFindingMatch", gameObject);
            LobbySystemManager.Instance.QuickPlay(this);
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
