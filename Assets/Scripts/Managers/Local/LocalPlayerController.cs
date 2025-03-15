using Core.Podium;
using Gameplay;
using Gameplay.UI;
using Managers.Network;
using Stats;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

//Player handles UI, and is the main interface for players...
namespace Managers.Local
{
    public class LocalPlayerController : MonoBehaviour
    {
        private BallPlayer _currentBall;
    
        [Header("Game")]
        [SerializeField] private Canvas rootCanvas;

        [Header("Controls UI")]
        [SerializeField] private Joystick halfJoystick;
        [SerializeField] private Joystick fullJoystick;
        [SerializeField] private AbilityHandler attackAbility;
        [SerializeField] private AbilityHandler specialAbility;

        [Header("Respawn UI")]
        [SerializeField] private GameObject respawnUI;
        [SerializeField] private TextMeshProUGUI killedByText;
        [SerializeField] private TextMeshProUGUI respawnTimerText;

        private Joystick _currentJoyStick;
        
        //Controls for local player...
        [Header("Controls")]
        [SerializeField] private CinemachineCamera cam;
        [SerializeField] private Canvas pauseCanvas;
        
        
        private PlayerInput _playerInput;
        private SpectatingManager _spectatingManager;

        //We can't let this be static for local multiplayer.
        public static LocalPlayerController LocalBallPlayer { get; private set; }
        public bool IsActive => _currentBall;


        private bool _tickRespawn;
        private float _respawnTimer;
        private const float RespawnTime = 5;

        private int _livesLeft;

        private void Awake()
        {
            if (LocalBallPlayer != null && LocalBallPlayer != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _playerInput = GetComponent<PlayerInput>();
            _spectatingManager = GetComponent<SpectatingManager>();

            InitializeControls();
            
            //most will be half, so default to that :P
            _currentJoyStick = halfJoystick;
            fullJoystick.gameObject.SetActive(false);
            halfJoystick.gameObject.SetActive(true);
            _livesLeft = 3;
            LocalBallPlayer = this;
            enabled = false;
        }

        private void InitializeControls()
        {
            if (_playerInput == null)
            {
                Debug.LogWarning("There was no player input on the player", gameObject);
                return;
            }
            
            //Gameplay Actions
            var useWeapon = _playerInput.actions["Weapon"];
            var useAbility = _playerInput.actions["Ability"];
            var controlSteering = _playerInput.actions["Steer"];
            var pauseGame = _playerInput.actions["EnterPause"];

            //UI Actions
            var unPauseGame = _playerInput.actions["ExitPause"];
            
            useWeapon.performed += ctx => TryDoWeapon(ctx.ReadValueAsButton());
            useAbility.performed += ctx => TryDoAbility(ctx.ReadValueAsButton());
            controlSteering.performed += ctx => SetSteer(ctx.ReadValue<Vector2>());
            pauseGame.performed += _ => TogglePauseState(true);
            unPauseGame.performed += _ => TogglePauseState(false);
            
            
          
        }

        private void TogglePauseState(bool state)
        {
            pauseCanvas.enabled = state;
        }

        private void OnEnable()
        {
            NetworkGameManager.Instance.OnGameEnd += OnGameEnded;
        }

        private void OnDisable()
        {
            NetworkGameManager.Instance.OnGameEnd -= OnGameEnded;

        }

        public void SwapJoySticks(bool isFull)
        {
            Joystick nextJoystick = isFull ? fullJoystick : halfJoystick;

            _currentJoyStick = nextJoystick;
            fullJoystick.gameObject.SetActive(nextJoystick == fullJoystick);
            halfJoystick.gameObject.SetActive(nextJoystick != fullJoystick);
        }

        private void OnGameEnded()
        {
            rootCanvas.enabled = false;
            respawnUI.SetActive(false);
            _spectatingManager.StopSpectating();
            _tickRespawn = false;
            DisableControls();
        }
        

        private void Update()
        {
            _currentBall.GetBall.Foward = cam.transform.forward;
            //_currentBall.GetBall.Foward.Value = cam.transform.forward;
            _currentBall.GetBall.MoveDirection = _currentJoyStick.Direction;
            //_currentBall.GetBall.MoveDirection.Value = joystick.Direction;

            //this should probs be done on the server....
            if (_tickRespawn)
            {
                _respawnTimer -= Time.deltaTime;

                respawnTimerText.text = ("RESPAWNING IN : " + (int)_respawnTimer);
                if (_respawnTimer <= 0)
                {
                    Unbind();
                    _tickRespawn = false;
                }
            }
        }

        #region Interaction
        public void EnableControls()
        {
            _currentJoyStick.enabled = true;
            //PlayerControls.EnableControls();
        }
    
        public void DisableControls()
        {
            _currentJoyStick.enabled = false;
            //PlayerControls.DisableControls();
        }
        private void TryDoAbility(bool state)
        {
            specialAbility.TryUseAbility(_currentBall.GetAbility, _currentBall);
        }

        private void TryDoWeapon(bool state)
        {
            attackAbility.TryUseAbility(_currentBall.GetWeapon.GetAbility, _currentBall);
        }

        private void SetSteer(Vector2 direction)
        {
            _currentJoyStick.SetInput(direction);
        }
        
        #endregion

        #region Initialization
        public void BindTo(BallPlayer ballPlayer)
        {
            _currentBall = ballPlayer; 
            
            SetBall(ballPlayer.GetBall); 
            SetAbilities(ballPlayer.GetWeapon,ballPlayer.GetAbility);
            enabled = true;
            rootCanvas.enabled = true;
            cam.enabled = true;
            
            SelectionMenu.Instance.EndDisplaying();

            ballPlayer.OnDestroyed += OnBallKilled;
            
            EnableControls();
        }

        private void OnBallKilled(ulong killer)
        {
            BallPlayer[] allBalls = FindObjectsByType<BallPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (BallPlayer ball in allBalls)
            {
                if (ball && ball.NetworkObject && ball.NetworkObject.OwnerClientId == killer)
                {
                    cam.LookAt =  ball.transform;
                    cam.Follow = null;
                    cam.InternalUpdateCameraState(Vector3.up, 10);
                }
            }
            respawnUI.SetActive(true);
            rootCanvas.enabled = false;

            killedByText.text = (killer == 100) ? "WORLD" : NetworkGameManager.Instance.GetPlayerName(killer);
            DisableControls();

            _tickRespawn = true;
            _respawnTimer = RespawnTime;
        }

        public void Unbind()
        {
            _livesLeft--;
            NetworkGameManager.Instance.FuckingLazyWayToDoThis_ServerRpc(NetworkGameManager.Instance.NetworkManager.LocalClientId, _livesLeft);

            if (_livesLeft <= 0)
            {
                _spectatingManager.StartSpectating();
                respawnUI.SetActive(false);
            }
            else
            {
                respawnUI.SetActive(false);
                rootCanvas.enabled = false;
                enabled = false;
                cam.enabled = false;
                SelectionMenu.Instance.BeginDisplaying();
            }

            //  DisableControls();
        }

        public void SetBall(Ball target)
        {
            if (cam.LookAt)
            {
          //      cam.LookAt = null;
              //  cam.InternalUpdateCameraState(Vector3.up, 10);
            }

            cam.LookAt = target.transform;
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
