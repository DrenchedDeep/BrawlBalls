using System;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class Ball : NetworkBehaviour, IDamageAble
{
    [SerializeField] private BallStats stats;
    [SerializeField] private Weapon weapon;
    [SerializeField] private AbilityStats ability;

    private Rigidbody rb;
    private float currentHealth;

    [SerializeField] private VisualEffect onDestroy;
    public Weapon Weapon => weapon; // I really don't want to have to do this...
    public AbilityStats SpecialAbility => ability;
    public float Acceleration => stats.Acceleration;
    public float MaxSpeed => stats.MaxSpeed;
    public float Drag => stats.Drag;
    

    private Player previousAttacker;


    public float Speed => rb.velocity.magnitude;
    
    

    private void Awake()
    {
        //if the ball is the local player?
        //if the PLAYER is the local player, then it should move THIS ball...
        if (IsLocalPlayer)
        {
            print("Am I local?");
        }

        rb = transform.GetChild(0).GetComponent<Rigidbody>();

        rb.drag = stats.Drag;
        rb.angularDrag = stats.AngularDrag;
        rb.mass = stats.Mass + weapon.Mass;

        currentHealth = stats.MaxHealth;
    }
    
    
    
    public void AddVelocity(Vector3 dir)
    {
        rb.AddForce(dir, ForceMode.Impulse);
    }

    public void TakeDamage(float amount, Vector3 direction, Player attacker)
    {
        currentHealth = Mathf.Max(currentHealth-amount, stats.MaxHealth);
        print( name + "Ouchie! I took damage: " + amount +",  " + direction);
        if (currentHealth <= 0)
        {
            previousAttacker = attacker;
            Die();
            //return;
        }
        rb.AddForce(direction, ForceMode.Impulse);

    }

    private bool isDead;

    private void Die()
    {
        if (!isDead) return;
        isDead = true;
        
        if (previousAttacker)
        {
            //previousAttacker.AwardKill();
            //Instantiate(onDestroy,transform.position,previousAttacker.transform.rotation);
        }
        //Destroy(gameObject);
    }
    
}
