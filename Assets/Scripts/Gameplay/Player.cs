using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

//Player handles UI, and is the main interface for players...
public class Player : MonoBehaviour
{
    [Header("Ball Object")]
    [SerializeField] private Ball[] balls;
    private Ball currentBall;
    
    
    [Header("UI")]
    [SerializeField] private Joystick joystick;
    [SerializeField] private Button abilityButton;
    [SerializeField] private Button attackButton;
    
   
    
    //Controls for local player...
    [SerializeField] private Transform camTrans;
    private Transform sphereTrans;
    private Rigidbody playerRb;

    public static Player LocalPlayer;
    public float BallY => sphereTrans.position.y;

    [SerializeField] private LayerMask groundLayers;
    
    
    //When the ball awakes, let's access our components and check what exists...
    void Start()
    {
        LocalPlayer = this;
        //TODO: This should activate, with UI
        SelectBall(0);
    }

    void StartUI()
    {
        //Toggle UI based on item abilites...
        abilityButton.gameObject.SetActive(currentBall.HasSpecialAbility);
        attackButton.gameObject.SetActive(currentBall.HasWeaponAbility);
    }


    // Update is called once per frame
    void Update()
    {
        //Only the PLAYER can move their ball...
        HandleDrag();
        HandleMovement();
        
    }

    void HandleDrag()
    {
        playerRb.drag = Physics.Raycast(sphereTrans.position, Vector3.down, 2, groundLayers)? currentBall.Drag : 0;

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
        playerRb = currentBall.PlayerRigidbody;
        sphereTrans = currentBall.PlayerTransform;

        CinemachineVirtualCamera cvc = camTrans.GetComponent<CinemachineVirtualCamera>();
        cvc.LookAt = currentBall.transform.GetChild(0);
        cvc.Follow = currentBall.transform.GetChild(1);
        
        
        StartUI();
    }

    public void UseAbility()
    {
        currentBall.UseSpecialAbility();
    }

    public void UseAttack()
    {
        print("Attack initiated");
        currentBall.UseWeaponAbility();
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

    public void SetWeaponAbilityState(bool state)
    {
        attackButton.gameObject.SetActive(state);
    }
}
