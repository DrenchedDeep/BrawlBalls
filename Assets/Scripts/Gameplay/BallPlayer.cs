using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

//Player handles UI, and is the main interface for players...
public class BallPlayer : MonoBehaviour
{
    private Ball[] balls;
    private Ball currentBall;
    private CinemachineVirtualCamera cvc;
    
    [Header("UI")]
    [SerializeField] private Joystick joystick;

    [SerializeField] private AbilityHandler attackAbility;
    [SerializeField] private AbilityHandler specialAbility;
    
    //Controls for local player...
    [SerializeField] private Transform camTrans;

    [SerializeField] private GameObject inGameUI;
    [SerializeField] private GameObject selectionUI;
    
    private Transform sphereTrans;
    private Rigidbody playerRb;

    public static BallPlayer LocalBallPlayer;
    public float BallY => sphereTrans.position.y;

    private int remainingBalls;
    private Vector3 initPos;
    private Vector3 rotation;

    public static bool alive { get; private set; }
    

    
    //When the ball awakes, let's access our components and check what exists...
    void Start()
    {
        LocalBallPlayer = this;
        //TODO: This should activate, with UI

        balls = GameManager.ConstructBalls();
        remainingBalls = balls.Length;
        cvc = camTrans.GetComponent<CinemachineVirtualCamera>();
        initPos = cvc.transform.position;
        rotation = cvc.transform.eulerAngles;

        //Camera stays in initial starting position, perhaps play a particle...

        //When a ball is selected, that's when we should call SelectBall(idx);

        //SelectBall(0);
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //Only the PLAYER can move their ball...
        if (!alive) return;
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
    public void SelectBall(int i)
    {
        //Trust user
        
        inGameUI.SetActive(true);
        selectionUI.SetActive(false);
        //currentBall = Instantiate(balls[i], (Level.Instance.IsRandomSpawning?SpawnPoint.ActiveSpawnPoints[Random.Range(0,SpawnPoint.ActiveSpawnPoints.Count)]:SpawnPoint.ActiveSpawnPoints[0]).transform.position + Vector3.up, Quaternion.identity);
        balls[i].Spawn();
        currentBall = balls[i];
        playerRb = currentBall.transform.GetChild(0).GetComponent<Rigidbody>();
        sphereTrans = currentBall.transform.GetChild(0);

       

        cvc.LookAt = sphereTrans;
        cvc.Follow =  currentBall.transform.GetChild(1);
        Weapon w = currentBall.Weapon;
        attackAbility.SetAbility(w.GetAbility, currentBall, w);
        specialAbility.SetAbility(currentBall.SpecialAbility, currentBall, w);
        alive = true;
    }
    #endregion

    public void Respawn(bool death)
    {
        alive = false;
        if(currentBall.gameObject)
            Destroy(currentBall.gameObject);
        //Start a timer
        if (death)
        {
           //Remove the current ball from the pool...
           //At this point the object is already destroyed..
           
           if (--remainingBalls <= 0)
           {
               print("Game ended, ran out of balls... Create UI");
               SceneManager.LoadScene(0);
           }
        }
        
        cvc.transform.position = initPos;
        cvc.transform.eulerAngles = rotation;
        
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
}
