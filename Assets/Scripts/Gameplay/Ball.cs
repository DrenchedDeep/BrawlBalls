using System;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class Ball : NetworkBehaviour, IDamageAble
{
    [SerializeField] private BallStats stats;
    [SerializeField] private Weapon weapon;
    [SerializeField] private SpecialAbility ability;

    private Rigidbody rb;
    private float currentHealth;

    [SerializeField] private VisualEffect onDestroy;
    
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
