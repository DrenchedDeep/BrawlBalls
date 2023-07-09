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

    [SerializeField] private VisualEffect destructionParticle;
    public Weapon Weapon => weapon; // I really don't want to have to do this...
    public AbilityStats SpecialAbility => ability;
    public float MaxSpeed => stats.MaxSpeed;
    
    private Player previousAttacker;

    private MeshRenderer mr;
    private int ballLayer;
    private int groundlayers;

    public float Speed => Velocity.magnitude;
    public Vector3 Velocity => rb.velocity;


     public float Acceleration { get; private set; }


     private void Awake()
    {
        //if the ball is the local player?
        //if the PLAYER is the local player, then it should move THIS ball...
        if (IsLocalPlayer)
        {
            print("Am I local?");
        }

        Transform t = transform.GetChild(0);
        
        groundlayers = GameManager.GroundLayers +  (1<< t.gameObject.layer);
        Acceleration = stats.Acceleration;

        rb = t.GetComponent<Rigidbody>();
        mr = t.GetComponent<MeshRenderer>();
        rb.drag = stats.Drag;
        rb.angularDrag = stats.AngularDrag;
        rb.mass = stats.Mass + weapon.Mass;

        currentHealth = stats.MaxHealth;
        print("HP: " + currentHealth);
    }

     private void FixedUpdate()
     {
         HandleDrag();
     }

     public void ChangeVelocity(Vector3 dir, ForceMode forceMode = ForceMode.Impulse, bool stop = false)
     {
         if (stop)
             rb.velocity = Vector3.zero;
        rb.AddForce(dir, forceMode);
    }

    /* Just take the collision point duh
    public void TakeDamage(float amount, float forceMul, Player attacker)
    {
        //Just push in negative direction
        TakeDamage(amount, -rb.velocity.normalized * forceMul, attacker);
    } */

    public void TakeDamage(float amount, Vector3 direction, Player attacker)
    {
        currentHealth = Mathf.Min(currentHealth-amount, stats.MaxHealth);
        print( name + "Ouchie! I took damage: " + amount +",  " + direction +", I have reamining health: " + currentHealth);
        if (currentHealth <= 0)
        {
            previousAttacker = attacker;
            Die();
            //return;
        }
        rb.AddForce(direction, ForceMode.Impulse);

    }

    private void HandleDrag()
    {
        bool hit = Physics.Raycast(rb.position, Vector3.down, out RaycastHit h, 1.5f, groundlayers);
        //#if UNITY_EDITOR
        Debug.DrawRay(rb.position, Vector3.down * 1.5f, hit?Color.blue:Color.yellow);
        //#endif
        //Handle squishing
        if (hit)
        {
            Transform n = h.transform.parent;
            if (n && n.TryGetComponent(out Ball b))
            {
                Debug.LogWarning("LANDED ON EM: " + b.name +", " + name);
                b.TakeDamage(1000000, Vector3.zero, Player.LocalPlayer);
                rb.drag = stats.Drag;
            }
            else
            {
                rb.drag =  0;
            }
        }
        else
        {
            rb.drag =  0;
        }

        
    }

    private bool isDead;

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        
        if (previousAttacker)
        {
            //previousAttacker.AwardKill();
            //Instantiate(onDestroy,transform.position,previousAttacker.transform.rotation);
        }
        print("Destroy!");
        Destroy(gameObject);
    }

    public void ApplySlow(Ball attacker, Material m)
    {
        //previousAttacker = attacker.owner
        Acceleration *= 0.7f;
        AddMaterial(m);
    }

    public int AddMaterial(Material mat)
    {
        int l = mr.materials.Length;
        Material[] mats = new Material[l+1];
        for (int index = 0; index < l; index++)
        {
            mats [index]= mr.materials[index];
        }
        mats[l]=mat;
        mr.materials = mats;
        return l;
    }

    public void RemoveMaterial(int id)
    {
        int l = mr.materials.Length;
        Material[] mats = new Material[l-1];
        int m = 0;
        for (int index = 0; index < l; index++)
        {
            if(index != id)
                mats [index] = mr.materials[m];
            m+=1;
        }
        mr.materials = mats;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        
    }
}
