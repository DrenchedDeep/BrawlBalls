using System;
using Core.ActionMaps;
using Managers.Local;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MainMenu
{
    [RequireComponent(typeof(PlayerInput))]
    public class MainMenuPlayer : MonoBehaviour
    {
        #if !UNITY_ANDROID && !UNITY_IOS
        PlayerInput _input;
        
        const string MainMenuActionMap = "MainMenuActionMap";
        const string UIActionMap =             "UIActionMap";

        public event Action<InputSpriteActionMap> OnDeviceChanged;
        public InputSpriteActionMap CurrentSpriteActionMap { get; private set; }

        private void Start()
        {
            _input = GetComponent<PlayerInput>();
            CurrentSpriteActionMap = ResourceManager.Instance.GetActionMap(_input.devices);
            OnDeviceChanged?.Invoke(CurrentSpriteActionMap);
            
            _input.onDeviceRegained += OnChangedDevice;
            _input.onDeviceLost += OnChangedDevice;
            
            //Okay so, what we want to do is make the game entirely controllable via buttons while controller is connected. We need to make selectable objects more clear.
            //Additionally, we want to add hotkeys to do some actions quicker.

        }

        private void OnChangedDevice(PlayerInput obj)
        {
            CurrentSpriteActionMap = ResourceManager.Instance.GetActionMap(obj.devices);
            OnDeviceChanged?.Invoke(CurrentSpriteActionMap);
        }
#endif
    }
}
