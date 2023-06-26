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
    [SerializeField] private Transform rotatorTrans;
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
        rb.AddForce(rotatorTrans.right*joystick.Horizontal, ForceMode.Acceleration);
        rb.AddForce(rotatorTrans.forward*joystick.Vertical, ForceMode.Acceleration);
        
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

    

}
