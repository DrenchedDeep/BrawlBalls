using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class Ball : NetworkBehaviour, IDamageAble
{
    [SerializeField] private BallStats stats;
    
    private AbilityStats _ability;
    private Weapon _weapon;
    private Rigidbody _rb;
    private float _currentHealth;

    [SerializeField] private VisualEffect destructionParticle;
    public Weapon Weapon => _weapon; // I really don't want to have to do this...

    public void SetWeapon(Weapon w)
    {
        _weapon = w;
        _weapon.transform.parent = transform;
    }

    public AbilityStats SpecialAbility => _ability;
    public void SetAbility(AbilityStats s) => _ability = s;
    
    
    
    public float MaxSpeed => stats.MaxSpeed;
    
    private BallPlayer previousAttacker;

    private MeshRenderer mr;
    private int ballLayer;
    private int groundlayers;

    public float Speed => Velocity.magnitude;
    public Vector3 Velocity => _rb.velocity;
    


     public float Acceleration { get; private set; }

     public override void OnNetworkSpawn()
     {
         base.OnNetworkSpawn();
         //if the ball is the local player?
         //if the PLAYER is the local player, then it should move THIS ball...
         if (IsLocalPlayer)
         {
             print("Am I local?");
         }

         Initialize();
     }

     private void Awake()
     {
         enabled = false;
     }

     private void Initialize()
     {
         Transform t = transform.GetChild(0);
         
         groundlayers = GameManager.GroundLayers +  (1<< t.gameObject.layer);
         Acceleration = stats.Acceleration;

         _rb = t.GetComponent<Rigidbody>();
         mr = t.GetComponent<MeshRenderer>();
         _rb.useGravity = true;
         _rb.drag = stats.Drag;
         _rb.angularDrag = stats.AngularDrag;
         _rb.mass = stats.Mass + _weapon.Mass;

         _currentHealth = stats.MaxHealth;
         print("HP: " + _currentHealth);
         enabled = true;
         //This isn't going to work because the object is a network object
         
     }

     public void Spawn()
     {
         
         if (GameManager.IsOnline)
         {
             NetworkObject.SpawnWithOwnership(NetworkManager.LocalClientId, true);
         }
         else
         {
             Initialize();
         }

         
         //transform.position = (Level.Instance.IsRandomSpawning ? SpawnPoint.ActiveSpawnPoints[Random.Range(0, SpawnPoint.ActiveSpawnPoints.Count)] : SpawnPoint.ActiveSpawnPoints[0]).transform.position + Vector3.up;
         Debug.Log(transform.position);
         
     }

     private void FixedUpdate()
     {
         HandleDrag();
     }

     public void ChangeVelocity(Vector3 dir, ForceMode forceMode = ForceMode.Impulse, bool stop = false)
     {
         if (stop)
             _rb.velocity = Vector3.zero;
        _rb.AddForce(dir, forceMode);
    }

    /* Just take the collision point duh
    public void TakeDamage(float amount, float forceMul, Player attacker)
    {
        //Just push in negative direction
        TakeDamage(amount, -rb.velocity.normalized * forceMul, attacker);
    } */

    public void TakeDamage(float amount, Vector3 direction, BallPlayer attacker)
    {
        _currentHealth = Mathf.Min(_currentHealth-amount, stats.MaxHealth);
        print( name + "Ouchie! I took damage: " + amount +",  " + direction +", I have reamining health: " + _currentHealth);
        if (_currentHealth <= 0)
        {
            previousAttacker = attacker;
            Die();
            //return;
        }
        _rb.AddForce(direction, ForceMode.Impulse);

    }

    private void HandleDrag()
    {
        bool hit = Physics.Raycast(_rb.position, Vector3.down, out RaycastHit h, 1.5f, groundlayers);
        //#if UNITY_EDITOR
        Debug.DrawRay(_rb.position, Vector3.down * 1.5f, hit?Color.blue:Color.yellow);
        //#endif
        //Handle squishing
        if (hit)
        {
            if ((1<<h.transform.gameObject.layer & GameManager.GroundLayers) != 0)
            {
                print("Hitting the floor...");
                _rb.drag = stats.Drag;
                return;
            }
            Transform n = h.transform.parent;
            if (n && n.TryGetComponent(out Ball b))
            {
                Debug.LogWarning("LANDED ON EM: " + b.name +", " + name);
                b.TakeDamage(1000000, Vector3.zero, BallPlayer.LocalBallPlayer);
            }
            else
            {
                _rb.drag =  0;
            }
        }
        else
        {
            _rb.drag =  0;
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
