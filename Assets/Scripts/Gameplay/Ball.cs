using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.VFX;

public class Ball : NetworkBehaviour, IDamageAble
{
    [SerializeField] private BallStats stats;
    [SerializeField] private Material baseMaterial;
    public Material BaseMaterial => baseMaterial; // :(
    private AbilityStats _ability;
    private Rigidbody _rb;
    private float _currentHealth;

    [SerializeField] private VisualEffect destructionParticle;


    public AbilityStats SpecialAbility => _ability;
    public void SetAbility(AbilityStats s) => _ability = s;
    
    
    
    public float MaxSpeed => stats.MaxSpeed;
    
    private ulong previousAttacker;

    private MeshRenderer mr;
    private int ballLayer;
    private int groundlayers;

    private Vector3 _previousPosition;
    private Vector3 curPos;
    public float Speed => Velocity.magnitude / Time.deltaTime;
    public Vector3 Velocity => curPos - _previousPosition;
    


     public float Acceleration { get; private set; }



     private void Initialize()
     {
         Transform t = transform.GetChild(1);
         
         groundlayers = GameManager.GroundLayers +  (1<< t.gameObject.layer);
         Acceleration = stats.Acceleration;

         _rb = t.GetComponent<Rigidbody>();
         mr = t.GetComponent<MeshRenderer>();
         _rb.useGravity = true;
         _rb.drag = stats.Drag;
         _rb.angularDrag = stats.AngularDrag;
         Weapon w = transform.GetChild(2).GetComponent<Weapon>();
         _rb.mass = stats.Mass + w.Mass;
         _currentHealth = stats.MaxHealth;
         print("HP: " + _currentHealth);
         enabled = true;
         mr.material = baseMaterial;
         NetworkObject.enabled = true;
         if (IsOwner)
         {
             print("I'm owned by local");
             BallPlayer.LocalBallPlayer.SetBall(this);
             BallPlayer.LocalBallPlayer.SetWeapon(w);
         }
         //This isn't going to work because the object is a network object

     }


     private void FixedUpdate()
     {
         HandleDrag();
         _previousPosition = curPos;
         curPos = _rb.position;
     }

     public void ChangeVelocity(Vector3 dir, ForceMode forceMode = ForceMode.Impulse, bool stop = false)
     {
         if (!IsOwner) return;
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

    public void TakeDamage(float amount, Vector3 direction, ulong attacker)
    {
        if (!IsOwner) return;
        
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
                _rb.drag = stats.Drag;
                return;
            }
            Transform n = h.transform.parent;
            if (n && n.TryGetComponent(out Ball b))
            {
                Debug.LogWarning("LANDED ON EM: " + b.name +", " + name);
                b.TakeDamage(1000000, Vector3.zero, ulong.MaxValue);
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


    private void Die()
    {
        
        //previousAttacker.AwardKill();
        //Instantiate(onDestroy,transform.position,previousAttacker.transform.rotation);
        print("Destroyed!");
        NetworkObject.Despawn();
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
    [ClientRpc]

    public void FinalizeClientRpc()
    {
        print("Loaded fully.");
        
        Initialize();

        transform.GetChild(0).GetComponent<PositionConstraint>().SetSource(0,new ConstraintSource{sourceTransform = transform.GetChild(1)});

    }
}
