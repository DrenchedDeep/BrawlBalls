using Cinemachine;
using Unity.Netcode;
using UnityEngine;

//Player handles UI, and is the main interface for players...
public class Player : MonoBehaviour
{
    [Header("Ball Object")]
    [SerializeField] private Ball[] balls;
    private Ball currentBall;
    
    
    [Header("UI")]
    [SerializeField] private Joystick joystick;

    [SerializeField] private AbilityHandler attackAbility;
    [SerializeField] private AbilityHandler specialAbility;
    
    //Controls for local player...
    [SerializeField] private Transform camTrans;
    private Transform sphereTrans;
    private Rigidbody playerRb;

    public static Player LocalPlayer;
    public float BallY => sphereTrans.position.y;

    
    //When the ball awakes, let's access our components and check what exists...
    void Start()
    {
        LocalPlayer = this;
        //TODO: This should activate, with UI
        SelectBall(0);
        
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //Only the PLAYER can move their ball...
        
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
        currentBall = Instantiate(balls[i], (Level.Instance.IsRandomSpawning?SpawnPoint.ActiveSpawnPoints[Random.Range(0,SpawnPoint.ActiveSpawnPoints.Count)]:SpawnPoint.ActiveSpawnPoints[0]).transform.position + Vector3.up, Quaternion.identity);
        playerRb = currentBall.transform.GetChild(0).GetComponent<Rigidbody>();
        sphereTrans = currentBall.transform.GetChild(0);

        CinemachineVirtualCamera cvc = camTrans.GetComponent<CinemachineVirtualCamera>();
        cvc.LookAt = sphereTrans;
        cvc.Follow =  currentBall.transform.GetChild(1);
        Weapon w = currentBall.Weapon;
        attackAbility.SetAbility(w.GetAbility, currentBall, w);
        specialAbility.SetAbility(currentBall.SpecialAbility, currentBall, w);
        
    }
    #endregion

    public void Respawn(bool b)
    {
        playerRb.velocity = Vector3.zero;
        if (!b)
        {
            sphereTrans.position = SpawnPoint.CurrentSpawnPoint.transform.position + Vector3.up;
        }
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
