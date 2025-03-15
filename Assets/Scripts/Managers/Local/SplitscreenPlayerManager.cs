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
        }


        public void SetLocalMultiplayerJoinable(bool state)
        {
            if(state) _playerInputManager.EnableJoining();
            else _playerInputManager.DisableJoining();
        }
        
        
    }
}
