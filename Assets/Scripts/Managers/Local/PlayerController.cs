using System;
using Core.Podium;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Spectating;
using Gameplay.UI;
using Gameplay.Weapons;
using Managers.Network;
using Stats;
using TMPro;
using Unity.Cinemachine;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//Player handles UI, and is the main interface for players...
namespace Managers.Local
{
    
    
    public class PlayerController : MonoBehaviour
    {
        private BallPlayer _currentBall;
    
        [Header("Canvas Control")]
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private Canvas gameCanvas;
        [SerializeField] private Canvas scoreBoardCanvas;
        [SerializeField] private Canvas selectionCanvas;
        [SerializeField] private Canvas pauseCanvas;
        [SerializeField] private Canvas spectateCanvas;
        [SerializeField] private Canvas deathCanvas;
        [SerializeField] private Canvas gameOverCanvas;


        [Header("Controls UI")]
        [SerializeField] private AssetStore.Joystick_Pack.Scripts.Base.Joystick halfJoystick;
        [SerializeField] private AssetStore.Joystick_Pack.Scripts.Base.Joystick fullJoystick;
        [SerializeField] private AbilityHandler attackAbility;
        [SerializeField] private AbilityHandler specialAbility;

        [Header("Respawn UI")]
        [SerializeField] private GameObject respawnUI;
        [SerializeField] private TextMeshProUGUI killedByText;
        [SerializeField] private TextMeshProUGUI respawnTimerText;

        private AssetStore.Joystick_Pack.Scripts.Base.Joystick _currentJoyStick;
        
        //Controls for local player...
        [Header("Controls")]
        [SerializeField] private CinemachineCamera cam;
        
        
        private PlayerInput _playerInput;
        public PlayerInput PlayerInput => _playerInput;
        private SpectatingManager _spectatingManager;
        public bool IsActive => _currentBall;


        private const float RespawnTime = 5;

        private int _livesLeft;

        private SelectionMenu _selectionMenu;

        private FixedString64Bytes _playerName;
        
        public FixedString64Bytes PlayerName => _playerName;

        //client id, child id -- who defeated us -- called when we run out of balls -- ONLY LOCALLY FOR THIS PLAYER
        public event Action<int, int> OnDefeated;
        
        
        //client id, child id -- who we defeated -- called when enemy runs out of balls -- ONLY LOCALLY FOR THIS PLAYER
        public event Action<int, int> OnEnemyDefeated;
        
        public GameState GameState { get; private set; } 
        

        private void OnEnable()
        {
            NetworkGameManager.Instance.OnGameStateUpdated += OnGameStateChanged;
            NetworkGameManager.Instance.OnGameEnd += OnGameEnded;
            NetworkGameManager.Instance.OnHostDisconnected += OnHostDisconnected;
        }

        private void OnDisable()
        {
            NetworkGameManager.Instance.OnGameStateUpdated -= OnGameStateChanged;
            NetworkGameManager.Instance.OnGameEnd -= OnGameEnded;
            NetworkGameManager.Instance.OnHostDisconnected -= OnHostDisconnected;
        }
        
        private void OnGameStateChanged(GameState obj)
        {
            switch (obj)
            {
                case GameState.WaitingForPlayers:
                    selectionCanvas.gameObject.SetActive(false);
                    break;
                
                case GameState.IntroCinematic:
                    selectionCanvas.gameObject.SetActive(false);
                    break;
                
                case GameState.SelectingBalls:
                    selectionCanvas.gameObject.SetActive(true);
                    break;
                
                case GameState.StartingGame:
                    selectionCanvas.gameObject.SetActive(false);
                    break;
                
                case GameState.InGame:
                    gameCanvas.gameObject.SetActive(true);
                    scoreBoardCanvas.gameObject.SetActive(false);
                    break;
                
                case GameState.EndingGame:
                    spectateCanvas.gameObject.SetActive(false);
                    deathCanvas.gameObject.SetActive(false);
                    gameCanvas.gameObject.SetActive(false);
                    deathCanvas.gameObject.SetActive(false);
                    gameCanvas.gameObject.SetActive(true);
                    scoreBoardCanvas.gameObject.SetActive(false);
                    break;
                
                case GameState.EndingGameCinematic:
                    break;
            }
        }

        
        private void Awake()
        {
            rootCanvas.enabled = false;
            _playerInput = GetComponent<PlayerInput>();
            _spectatingManager = GetComponent<SpectatingManager>();
            
            //most will be half, so default to that :P
            _currentJoyStick = fullJoystick;
            fullJoystick.gameObject.SetActive(true);
           // halfJoystick.gameObject.SetActive(true);
            _livesLeft = 3;
            
            InitializeControls();
            enabled = false;
        }
        
        private void InitializeControls()
        {
            if (_playerInput == null)
            {
                Debug.LogWarning("There was no player input on the player", gameObject);
                return;
            }
            
            
            _playerInput.actions["Weapon"].performed += ctx => TryDoWeapon(ctx.ReadValueAsButton());
            _playerInput.actions["Ability"].performed += ctx => TryDoAbility(ctx.ReadValueAsButton());
            _playerInput.actions["Steer"].performed += ctx => SetSteer(ctx.ReadValue<Vector2>());
            _playerInput.actions["EnterPause"].performed += _ => TogglePauseState(true);
            _playerInput.actions["ExitPause"].performed += _ => TogglePauseState(false);
            DisableControls();
            
          
        }

        private void TogglePauseState(bool state)
        {
//            pauseCanvas.enabled = state;
        }



        private void OnGameEnded()
        {
            Debug.Log("game end?");
            rootCanvas.enabled = false;
            respawnUI.SetActive(false);
            _spectatingManager.StopSpectating();
            _spectatingManager.enabled = false;
            DisableControls();
        }
        
        private void OnHostDisconnected()
        {
            //other logic... let the player know?? for now just return to main menu
            ReturnToMainMenu();
        }


        public void ReturnToMainMenu()
        {
            Debug.Log("client is returning to main menu!");
            SceneManager.LoadScene("MainMenuNEW");
        }

        public void OnKilledPlayer(int clientID, int childID, string victimName)
        {
            OnEnemyDefeated?.Invoke(clientID, childID);

            if (TryGetComponent(out PlayerHUD hud))
            {
                hud.OnKilledPlayer(victimName);
            }


        }
        

        private void Update()
        {
            Vector3 x = cam.transform.forward;
            x.y = 0;
            _currentBall.GetBall.Foward.Value = x.normalized;
            _currentBall.GetBall.MoveDirection.Value = _currentJoyStick.Direction;
        }

        #region Interaction
        public void EnableControls()
        {
            rootCanvas.enabled = true;
            //PlayerControls.EnableControls();
            _playerInput.currentActionMap.Enable();
           Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            _playerInput.SwitchCurrentActionMap("Game");
        }
    
        public void DisableControls()
        {
            rootCanvas.enabled = false;
            TryDoAbility(false);
            TryDoWeapon(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            SetSteer(Vector2.zero);
            //PlayerControls.DisableControls();
            _playerInput.SwitchCurrentActionMap("UI");

        }
        private void TryDoAbility(bool state)
        {
            if(IsActive) specialAbility.SetUsingState(state);
        }

        private void TryDoWeapon(bool state)
        {
            if(IsActive) attackAbility.SetUsingState(state); 
            
        }

        private void SetSteer(Vector2 direction)
        {
            #if !(UNITY_ANDROID || UNITY_IOS) || UNITY_EDITOR
            if(IsActive) _currentJoyStick.SetInput(direction);
            #endif
        }
        
        #endregion

        #region Initialization
        public void BindTo(BallPlayer ballPlayer)
        {
            _currentBall = ballPlayer; 
            
            SetBall(ballPlayer); 
            SetAbilities(ballPlayer.GetBaseWeapon,ballPlayer.GetAbility);
            
            enabled = true;
            rootCanvas.enabled = true;
            cam.enabled = true;
            
            _selectionMenu.EndDisplaying();

            ballPlayer.OnDestroyed += OnBallKilled;
            
            EnableControls();
        }

        private void OnBallKilled(ulong killer, int childID)
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

            killedByText.text = (killer == 100) ? "WORLD" : NetworkGameManager.Instance.GetPlayerName(killer, childID);
            DisableControls();

            _ = ProcessRespawn();
        }

        private async UniTask ProcessRespawn()
        {
            float respawnTime = RespawnTime;
            while (respawnTime > 0)
            {
                respawnTime -= Time.deltaTime;
                respawnTimerText.text = ("RESPAWNING IN : " + (int)respawnTime);
                await UniTask.Yield();
            }

            respawnTimerText.text = ("RESPAWNING IN : " + 0);
            Unbind();
        }

        public void Unbind()
        {
            _livesLeft--;
            NetworkGameManager.Instance.FuckingLazyWayToDoThis_ServerRpc(NetworkGameManager.Instance.NetworkManager.LocalClientId, PlayerInput.playerIndex);

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
                _selectionMenu.BeginDisplaying();
            }

            //  DisableControls();
        }

        public void SetBall(BallPlayer target)
        {
            cam.LookAt = target.transform;
            cam.Follow =  target.transform;
            cam.InternalUpdateCameraState(Vector3.up, 10);
        }

        private void SetAbilities(BaseWeapon w, AbilityStats ballPlayerGetAbility)
        {
            attackAbility.SetAbility(w.GetAbility, _currentBall);
            specialAbility.SetAbility(ballPlayerGetAbility, _currentBall);
        }
        #endregion

        public void SetSelectionMenu(SelectionMenu selectionMenu)
        {
            _selectionMenu = selectionMenu;
        }
    }
}
