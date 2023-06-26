using System;
using UnityEngine;
using UnityEngine.UI;

//Player handles UI, and is the main interface for players...
public class Player : MonoBehaviour
{
    [Header("Ball Object")]
    [SerializeField] private BallStats stats;
    [SerializeField] private Weapon[] weapon;
    [SerializeField] private SpecialAbility[] specialAbility;
    
    private Weapon currentWeapon;
    private SpecialAbility currentAbility;
    
    //TODO: only the current player needs this.
    [Header("UI")]
    [SerializeField] private Joystick joystick;
    [SerializeField] private Button abilityButton;
    [SerializeField] private Button attackButton;
    
    private Rigidbody rb;
    [SerializeField] private Transform weaponTrans;
    [SerializeField] private Transform rotatorTrans;
    [SerializeField] private Transform objectTrans;

    public static Player LocalPlayer;
    public float BallY => objectTrans.position.y;
    
    
    //When the ball awakes, let's access our components and check what exists...
    void Start()
    {
        LocalPlayer = this;
        
        //TODO: fix
        rb = transform.GetChild(1).GetComponent<Rigidbody>();
        
        
        //TODO: This should activate, with UI
        SelectBall(0);
    }

    void StartUI()
    {
        //Toggle UI based on item abilites...
        abilityButton.gameObject.SetActive(currentWeapon.HasAbility);
        attackButton.gameObject.SetActive(currentAbility.HasAbility);
    }


    // Update is called once per frame
    void Update()
    {
        HandleMovement();

    }

    void HandleMovement()
    {
       //Move player...
        //rb.AddForce(joystick.Horizontal * stats.Acceleration * rotatorTrans.right, ForceMode.Acceleration );
        //rb.AddForce(joystick.Vertical * stats.Acceleration * rotatorTrans.forward, ForceMode.Acceleration);
        
        //Instead of using the ball, let's use the camera..... The forward and right of the camera, based on the world 

        Vector3 fwd = (objectTrans.position-rotatorTrans.position).normalized;
        fwd.y = 0;
        print(fwd);
        
        
            
        rb.AddForce(joystick.Vertical * stats.Acceleration * fwd, ForceMode.Acceleration);
        rb.AddForce(joystick.Horizontal * stats.Acceleration * Vector3.Cross( Vector3.up,fwd), ForceMode.Acceleration);
        
        Vector3 velocity = rb.velocity;

        Vector3 dir = Vector3.Lerp(Vector3.up, velocity.normalized, velocity.sqrMagnitude);
        weaponTrans.position = objectTrans.position + dir * 0.6f;
        weaponTrans.forward = dir;
    }


    /*

    private void OnCollisionStay(Collision other)
    {
        other.GetContact(0).normal
    }*/


    //For later (Selection)
    public void SelectBall(int i)
    {
        //Trust user
        currentWeapon = weapon[i];
        currentAbility = specialAbility[i];
        StartUI();
    }

    public void UseAbility()
    {
        
    }

    public void UseAttack()
    {
        
    }


    public void TakeDamage(float i)
    {
        
    }

    public void Respawn(bool b)
    {
        rb.velocity = Vector3.zero;
        if (!b)
        {
            objectTrans.position = SpawnPoint.CurrentSpawnPoint.transform.position + Vector3.up;
        }
        
    }
}
