using Managers;
using Managers.Controls;
using UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

//Player handles UI, and is the main interface for players...
namespace Gameplay.Balls
{
    public class BallPlayer : MonoBehaviour
    {
        private NetworkBall[] _balls;
        private NetworkBall _currentNetworkBall;
    
        [Header("UI")]
        [SerializeField] private Joystick joystick;

        [SerializeField] private AbilityHandler attackAbility;
        [SerializeField] private AbilityHandler specialAbility;
    
        //Controls for local player...
        [SerializeField] private Transform camTrans;

        [SerializeField] private GameObject inGameUI;
        [SerializeField] private GameObject selectionUI;
    
        [Header("Cinemachine")]
        [SerializeField] private CinemachineCamera cinemachinePrecam;
        [SerializeField] private CinemachineCamera cinemachineSelectcam;
        [SerializeField] private CinemachineCamera cinemachinePostcam;
    
        private Transform _sphereTrans;
        private Rigidbody _playerRb;

        public static BallPlayer LocalBallPlayer { get; private set; }
        public float BallY => _sphereTrans.position.y;

        private int _remainingBalls;
        private Vector3 _initPos;
        private Vector3 _rotation;

        public static bool Alive { get; private set; }

        private void Awake()
        {
            LocalBallPlayer = this;
        }

        private void Start()
        {
            HandleCams(0);
            selectionUI.SetActive(false);
        }

        private void HandleCams(int currentCam)
        {
            switch (currentCam)
            {
                case 0:
                    //cinemachinePrecam.enabled=true;
                    //cinemachineSelectcam.enabled=false;
                    cinemachinePostcam.enabled=false;
                    break;
                case 1:
                    //cinemachinePrecam.enabled=false;
                    //cinemachineSelectcam.enabled=true;
                    cinemachinePostcam.enabled=false;
                    break;
                case 2:
                    //cinemachinePrecam.enabled=false;
                    //cinemachineSelectcam.enabled=false;
                    cinemachinePostcam.enabled=true;
                    break;
            }
        }

        //When the ball awakes, let's access our components and check what exists...
        public void Initialize()
        {
            //TODO: This should activate, with UI
            HandleCams(1);
            _balls = BallHandler.Instance.SpawnBalls();
            selectionUI.SetActive(true);
        }


        void FixedUpdate()
        {
            //Only the PLAYER can move their ball...
            if (!Alive) return;
            HandleMovement();
        
        }


        //Would this be an RPC?
        void HandleMovement()
        {
            Vector3 fwd = (_sphereTrans.position-camTrans.position).normalized;
            fwd.y = 0;
        
            _playerRb.AddForce(joystick.Vertical * _currentNetworkBall.Acceleration * fwd, ForceMode.Acceleration);
            _playerRb.AddForce(joystick.Horizontal * _currentNetworkBall.Acceleration * Vector3.Cross( Vector3.up,fwd), ForceMode.Acceleration);
        
            Vector3 velocity = _playerRb.linearVelocity;

            float y = velocity.y;
            velocity.y = 0;
            //Limit velocity...
            //Memory or CPU?
            _playerRb.linearVelocity = Vector3.ClampMagnitude(velocity, _currentNetworkBall.MaxSpeed) + Vector3.up * y; //maintain our Y
        }


        #region UI_Button_Integration

        private int sel;
        public void SelectBall(int i)
        {
            //Trust user
            sel = i;
            print("Selecting ball: " +PlayerBallInfo.Balls[i].Ball );
            inGameUI.SetActive(true);
            selectionUI.SetActive(false);
        
            BallHandler.Instance.SpawnBallServerRpc(PlayerBallInfo.Balls[i].Ball,PlayerBallInfo.Balls[i].Weapon);
        
            //currentBall = Instantiate(balls[i], (Level.Instance.IsRandomSpawning?SpawnPoint.ActiveSpawnPoints[Random.Range(0,SpawnPoint.ActiveSpawnPoints.Count)]:SpawnPoint.ActiveSpawnPoints[0]).transform.position + Vector3.up, Quaternion.identity);
            //balls[i].SetAbility();
        
        }
        #endregion

        public void Respawn(bool death)
        {
        
            Alive = false;
            //if(currentBall.gameObject)
            //    Destroy(currentBall.gameObject);
            //Start a timer
            if (death)
            {
                //Remove the current ball from the pool...
                //At this point the object is already destroyed..
           
                if (--_remainingBalls <= 0)
                {
                    print("Game ended, ran out of balls... Create UI");
                    SceneManager.LoadScene(0);
                }
            }
            inGameUI.SetActive(false);
            selectionUI.SetActive(true);
        }


        public void EnableControls()
        {
            joystick.enabled = true;
            PlayerControls.EnableControls();

        }
    
        public void DisableControls()
        {
            joystick.enabled = false;
            PlayerControls.DisableControls();
        }

        public void SetBall(Balls.NetworkBall networkBall)
        {
            _currentNetworkBall = networkBall;
            _playerRb = _currentNetworkBall.transform.GetChild(1).GetComponent<Rigidbody>();
            _sphereTrans = _currentNetworkBall.transform.GetChild(1);
        
            cinemachinePostcam.LookAt = _sphereTrans;
            cinemachinePostcam.Follow =  _currentNetworkBall.transform.GetChild(1);
            Alive = true;
        
            HandleCams(2);
        }

        public void SetWeapon(Weapon w)
        {
            attackAbility.SetAbility(w.GetAbility, _currentNetworkBall, w);
            _currentNetworkBall.SetAbility(GameManager.Abilities[PlayerBallInfo.Balls[sel].Ability]);
            specialAbility.SetAbility(_currentNetworkBall.SpecialAbility, _currentNetworkBall, w);
        }

        public void TryDoAbility(bool state)
        {
            
        }

        public void TryDoWeapon(bool state)
        {
            
        }

        public void SetSteer(Vector2 direction)
        {
            
        }
    }
}
