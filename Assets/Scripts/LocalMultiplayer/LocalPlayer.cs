using System;
using System.Collections;
using Core.ActionMaps;
using Loading;
using Managers.Local;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LocalMultiplayer
{
    /// <summary>
    /// This classes purpose is a driver, it relays information to other necessary classes, and handles core local player functionality
    /// This class should be placed on the root object for the spawned client. 
    /// </summary>
    public class LocalPlayer : MonoBehaviour
    {
        
        public event Action<InputSpriteActionMap> OnDeviceChanged;
        public InputSpriteActionMap CurrentSpriteActionMap { get; private set; }
        
        public PlayerInput input;

        private IEnumerator Start()
        {
            yield return new WaitWhile(() => LoadingController.IsLoading);
            
            if (!SaveManager.TryGetPlayerData(input, out SaveManager.PlayerData dat))
            {
                Debug.LogError("We tried loading a local player before completing the intialization step", gameObject);
            }
            name = dat.Username;
        }


        private void OnChangedDevice(PlayerInput obj)
        {
            CurrentSpriteActionMap = ResourceManager.Instance.GetActionMap(obj.devices);
            OnDeviceChanged?.Invoke(CurrentSpriteActionMap);
        }
    }
}
