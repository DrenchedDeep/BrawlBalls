using System;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

//Player handles UI, and is the main interface for players...
public class BallPlayer : MonoBehaviour
{
    private Ball[] _balls;
    private Ball currentBall;
    
    [Header("UI")]
    [SerializeField] private Joystick joystick;

    [SerializeField] private AbilityHandler attackAbility;
    [SerializeField] private AbilityHandler specialAbility;
    
    //Controls for local player...
    [SerializeField] private Transform camTrans;

    [SerializeField] private GameObject inGameUI;
    [SerializeField] private GameObject selectionUI;
    
    [Header("Cinemachine")]
    [SerializeField] private GameObject cinemachinePrecam;
    [SerializeField] private GameObject cinemachineSelectcam;
    [SerializeField] private CinemachineVirtualCamera cinemachinePostcam;
    
    private Transform sphereTrans;
    private Rigidbody playerRb;

    public static BallPlayer LocalBallPlayer;
    public float BallY => sphereTrans.position.y;

    private int _remainingBalls;
    private Vector3 initPos;
    private Vector3 rotation;

    public static bool Alive { get; private set; }

    private void Awake()
    {
        LocalBallPlayer = this;
        HandleCams(0);
        selectionUI.SetActive(false);
    }

    private void HandleCams(int currentCam)
    {
        switch (currentCam)
        {
            case 0:
                cinemachinePrecam.SetActive(true);
                cinemachineSelectcam.SetActive(false);
                cinemachinePostcam.gameObject.SetActive(false);
                break;
            case 1:
                cinemachinePrecam.SetActive(false);
                cinemachineSelectcam.SetActive(true);
                cinemachinePostcam.gameObject.SetActive(false);
                break;
            case 2:
                cinemachinePrecam.SetActive(false);
                cinemachineSelectcam.SetActive(false);
                cinemachinePostcam.gameObject.SetActive(true);
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
        Vector3 fwd = (sphereTrans.position-camTrans.position).normalized;
        fwd.y = 0;
        
        playerRb.AddForce(joystick.Vertical * currentBall.Acceleration * fwd, ForceMode.Acceleration);
        playerRb.AddForce(joystick.Horizontal * currentBall.Acceleration * Vector3.Cross( Vector3.up,fwd), ForceMode.Acceleration);
        
        Vector3 velocity = playerRb.velocity;

        float y = velocity.y;
        velocity.y = 0;
        //Limit velocity...
        //Memory or CPU?
        playerRb.velocity = Vector3.ClampMagnitude(velocity, currentBall.MaxSpeed) + Vector3.up * y; //maintain our Y
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
        
    }
    
    public void DisableControls()
    {
        joystick.enabled = false;
    }

    public void SetBall(Ball ball)
    {
        currentBall = ball;
        playerRb = currentBall.transform.GetChild(1).GetComponent<Rigidbody>();
        sphereTrans = currentBall.transform.GetChild(1);
        
        cinemachinePostcam.LookAt = sphereTrans;
        cinemachinePostcam.Follow =  currentBall.transform.GetChild(1);
        Alive = true;
        
        HandleCams(2);
    }

    public void SetWeapon(Weapon w)
    {
        attackAbility.SetAbility(w.GetAbility, currentBall, w);
        currentBall.SetAbility(GameManager.Abilities[PlayerBallInfo.Balls[sel].Ability]);
        specialAbility.SetAbility(currentBall.SpecialAbility, currentBall, w);
    }

}
