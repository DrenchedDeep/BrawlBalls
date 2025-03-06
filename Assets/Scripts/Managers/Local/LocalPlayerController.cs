using System;
using Gameplay;
using Gameplay.UI;
using Managers.Controls;
using Stats;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

//Player handles UI, and is the main interface for players...
namespace Managers.Local
{
    public class LocalPlayerController : MonoBehaviour
    {
        private BallPlayer _currentBall;
    
        [Header("Game")]
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private GameObject selectionMenu;
        
        [Header("UI")]
        [SerializeField] private Joystick joystick;
        [SerializeField] private AbilityHandler attackAbility;
        [SerializeField] private AbilityHandler specialAbility;
    
        //Controls for local player...
        [Header("Controls")]
        [SerializeField] private CinemachineCamera cam;
    

        //We can't let this be static for local multiplayer.
        public static LocalPlayerController LocalBallPlayer { get; private set; }
        public bool IsActive => _currentBall;

        private void Awake()
        {
            if (LocalBallPlayer != null && LocalBallPlayer != this)
            {
                Destroy(gameObject);
                return;
            }
            LocalBallPlayer = this;
            enabled = false;
        }

        private void Update()
        {
            _currentBall.GetBall.Foward.Value = cam.transform.forward;
            _currentBall.GetBall.MoveDirection.Value = joystick.Direction;
        }

        public void IncreaseScore()
        {
        }


        #region Interaction
        public void EnableControls()
        {
            joystick.enabled = true;
            //PlayerControls.EnableControls();
        }
    
        public void DisableControls()
        {
            joystick.enabled = false;
            //PlayerControls.DisableControls();
        }
        public void TryDoAbility(bool state)
        {
            specialAbility.TryUseAbility(_currentBall.GetAbility, _currentBall);
        }

        public void TryDoWeapon(bool state)
        {
            attackAbility.TryUseAbility(_currentBall.GetWeapon.GetAbility, _currentBall);
        }

        public void SetSteer(Vector2 direction)
        {
            joystick.HandleInput(direction.magnitude, direction.normalized);
        }
        
        #endregion

        #region Initialization
        public void BindTo(BallPlayer ballPlayer)
        {
            Debug.Log("I am owned locally: ", ballPlayer);
            
            _currentBall = ballPlayer; 
            ballPlayer.name = "Local Player Ball"; 
            
            SetBall(ballPlayer.GetBall); 
            SetAbilities(ballPlayer.GetWeapon,ballPlayer.GetAbility);
            enabled = true;
            rootCanvas.enabled = true;
            cam.enabled = true;

            ballPlayer.OnDestroyed += _ => Unbind();
        }

        public void Unbind()
        {
            rootCanvas.enabled = false;
            enabled = false;
            cam.enabled = false;
            selectionMenu.SetActive(true);
        }

        private void SetBall(Ball target)
        {
            cam.Follow =  target.transform;
            cam.InternalUpdateCameraState(Vector3.up, 10);
        }

        private void SetAbilities(Weapon w, AbilityStats ballPlayerGetAbility)
        {
            if(w.GetAbility) attackAbility.SetAbility(w.GetAbility, _currentBall);
            specialAbility.SetAbility(ballPlayerGetAbility, _currentBall);
        }
        #endregion
    }
}
