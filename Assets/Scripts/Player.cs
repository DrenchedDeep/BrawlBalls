using System;
using UnityEngine;
using UnityEngine.UI;

//Player handles UI, and is the main interface for players...
public class Player : MonoBehaviour
{
    [Header("Ball Object")]
    [SerializeField] private Weapon[] weapon;
    [SerializeField] private SpecialAbility[] specialAbility;
    
    private Weapon currentWeapon;
    private SpecialAbility currentAbility;
    
    //TODO: only the current player needs this.
    [Header("UI")]
    [SerializeField] private DynamicJoystick joystick;
    [SerializeField] private Button abilityButton;
    [SerializeField] private Button attackButton;
    
    private Rigidbody rb;
    [SerializeField] private Transform weaponTrans;
    [SerializeField] private Transform objectTrans;
    
    
    //When the ball awakes, let's access our components and check what exists...
    void Start()
    {
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
        rb.AddForce(weaponTrans.right*joystick.Horizontal, ForceMode.Acceleration);
       // rb.AddForce(Vector3.right*joystick.Horizontal, ForceMode.Acceleration);
        rb.AddForce(weaponTrans.forward*joystick.Vertical, ForceMode.Acceleration);
        //rb.AddForce(Vector3.forward*joystick.Vertical, ForceMode.Acceleration);

        Vector3 velocity = rb.velocity;
        //:clown:
        weaponTrans.position = objectTrans.position + velocity.normalized * 0.6f;
        weaponTrans.forward = velocity.normalized;
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

    

}
