using Core.Podium;
using Gameplay;
using Gameplay.UI;
using Managers.Network;
using Stats;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

//Player handles UI, and is the main interface for players...
namespace Managers.Local
{
    public class LocalPlayerController : MonoBehaviour
    {
        private BallPlayer _currentBall;
    
        [Header("Game")]
        [SerializeField] private Canvas rootCanvas;

        [Header("Controls UI")]
        [SerializeField] private Joystick joystick;
        [SerializeField] private AbilityHandler attackAbility;
        [SerializeField] private AbilityHandler specialAbility;

        [Header("Respawn UI")]
        [SerializeField] private GameObject respawnUI;
        [SerializeField] private TextMeshProUGUI killedByText;
        [SerializeField] private TextMeshProUGUI respawnTimerText;

        //Controls for local player...
        [Header("Controls")]
        [SerializeField] private CinemachineCamera cam;
    

        //We can't let this be static for local multiplayer.
        public static LocalPlayerController LocalBallPlayer { get; private set; }
        public bool IsActive => _currentBall;


        private bool _tickRespawn;
        private float _respawnTimer;
        private const float RespawnTime = 5;

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

        private void OnEnable()
        {
            NetworkGameManager.Instance.OnGameEnd += OnGameEnded;
        }

        private void OnDisable()
        {
            NetworkGameManager.Instance.OnGameEnd -= OnGameEnded;

        }

        private void OnGameEnded()
        {
            rootCanvas.enabled = false;
            DisableControls();
        }

        private void Update()
        {
            _currentBall.GetBall.Foward.Value = cam.transform.forward;
            _currentBall.GetBall.MoveDirection.Value = joystick.Direction;

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
            
            SelectionMenu.Instance.EndDisplaying();

            ballPlayer.OnDestroyed += OnBallKilled;
            
            EnableControls();
        }

        private void OnBallKilled(ulong killer)
        {
            BallPlayer[] allBalls = FindObjectsByType<BallPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (BallPlayer ball in allBalls)
            {
                if (ball.NetworkObject.OwnerClientId == killer)
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
            respawnUI.SetActive(false);
            rootCanvas.enabled = false;
            enabled = false;
            cam.enabled = false;
            SelectionMenu.Instance.BeginDisplaying();
            
          //  DisableControls();
        }

        private void SetBall(Ball target)
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
