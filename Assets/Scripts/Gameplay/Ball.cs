using System;
using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour, IDamageAble
{
    [SerializeField] private BallStats stats;
    [SerializeField] private Weapon weapon;
    [SerializeField] private SpecialAbility ability;

    private Rigidbody rb;
    private float currentHealth;
    
    public bool HasWeaponAbility => weapon.HasAbility;
    public bool HasSpecialAbility => ability.HasAbility;
    public float Acceleration => stats.Acceleration;
    public float MaxSpeed => stats.MaxSpeed;
    public float Drag => stats.Drag;
    

    private Player previousAttacker;
    public Rigidbody PlayerRigidbody => rb;
    public Transform PlayerTransform => ability.transform;

    private void Awake()
    {
        //if the ball is the local player?
        //if the PLAYER is the local player, then it should move THIS ball...
        if (IsLocalPlayer)
        {
            print("Am I local?");
        }

        rb = ability.transform.GetComponent<Rigidbody>();

        rb.drag = stats.Drag;
        rb.angularDrag = stats.AngularDrag;
        rb.mass = stats.Mass + weapon.Mass;

        currentHealth = stats.MaxHealth;
    }

    

    public void UseWeaponAbility()
    {
        weapon.UseAbility();
    }
    
    public void UseSpecialAbility()
    {
        
    }


    public void TakeDamage(float amount, Vector3 direction, Player attacker)
    {
        currentHealth = Mathf.Max(currentHealth-amount, stats.MaxHealth);
        if (currentHealth <= 0)
        {
            previousAttacker = attacker;
            Die();
            return;
        }
        rb.AddForce(direction, ForceMode.Impulse);

    }

    private void Die()
    {
        if (previousAttacker)
        {
            //previousAttacker.AwardKill();
        }
    }
    
}
