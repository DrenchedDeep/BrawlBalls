using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

public class Ball : NetworkBehaviour, IDamageAble
{
    [SerializeField] private BallStats stats;
    public Material BaseMaterial => stats.Material; // :(
    public Mesh BaseMesh => stats.Mesh; // :(
    private AbilityStats _ability;
    private Rigidbody _rb;
    private float _currentHealth;

    public AbilityStats SpecialAbility => _ability;
    public void SetAbility(AbilityStats s) => _ability = s;


    public float MaxSpeed => stats.MaxSpeed;

    private ulong previousAttacker;

    private MeshRenderer mr;
    private int ballLayer;
    private int groundlayers;

    private Vector3 _previousPosition;
    private Vector3 curPos;
    public float Speed { get; private set; }
    public Vector3 Velocity { get; private set; }


    public float Acceleration { get; private set; }


    private void Initialize()
    {
        Transform t = transform.GetChild(1);

        groundlayers = GameManager.GroundLayers + (1 << t.gameObject.layer);
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
        mr.material = BaseMaterial;
        t.GetComponent<MeshFilter>().mesh = BaseMesh;
        NetworkObject.enabled = true;
        _rb.gameObject.layer = IsOwner?GameManager.LocalLayer:GameManager.EnemyLayer;
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
        if (!IsOwner) return;

        HandleDrag();
        //Works only for client
        _previousPosition = curPos;
        curPos = _rb.position;
        Velocity = curPos - _previousPosition;
        Speed = Velocity.magnitude / Time.deltaTime;
    }

    private void Update()
    {
        if (IsOwner) return;

        _previousPosition = curPos;
        curPos = _rb.position;
        Velocity = curPos - _previousPosition;
        Speed = Velocity.magnitude / Time.deltaTime;
    }

    public void ChangeVelocity(Vector3 dir, ForceMode forceMode = ForceMode.Impulse, bool stop = false)
    {
        if (!IsOwner) return;
        if (stop)
            _rb.velocity = Vector3.zero;
        _rb.AddForce(dir, forceMode);
    }

    [ClientRpc]
    public void TakeDamageClientRpc(float amount, Vector3 direction, ulong attacker)
    {
        print("Check damage: " + amount);
        if (!IsOwner) return;

        _currentHealth -= amount;
        print(name + "Ouchie! I took damage: " + amount + ",  " + direction + ", I have reamining health: " +
              _currentHealth);
        if (_currentHealth <= 0)
        {
            previousAttacker = attacker;
            MessageHandler.SetScreenMessage("Died to: <color=#ff000>" + attacker + "</color>", 3f);
            DieServerRpc(attacker);
            BallPlayer.LocalBallPlayer.Respawn(false);
            //return;
        }

        _rb.AddForce(direction, ForceMode.Impulse);
    }

    private void HandleDrag()
    {
        bool hit = Physics.Raycast(_rb.position, Vector3.down, out RaycastHit h, 1.5f, groundlayers);
        //#if UNITY_EDITOR
        Debug.DrawRay(_rb.position, Vector3.down * 1.5f, hit ? Color.blue : Color.yellow);
        //#endif
        //Handle squishing
        if (hit)
        {
            if ((1 << h.transform.gameObject.layer & GameManager.GroundLayers) != 0)
            {
                _rb.drag = stats.Drag;
                return;
            }

            Transform n = h.transform.parent;
            if (n && n.TryGetComponent(out Ball b))
            {
                Debug.LogWarning("LANDED ON EM: " + b.name + ", " + name);
                b.TakeDamageClientRpc(1000000, Vector3.zero, ulong.MaxValue);
            }
            else
            {
                _rb.drag = 0;
            }
        }
        else
        {
            _rb.drag = 0;
        }
    }


    [ServerRpc]
    private void DieServerRpc(ulong killer)
    {
        //previousAttacker.AwardKill();
        //Instantiate(onDestroy,transform.position,previousAttacker.transform.rotation);
        //Because it needs to be parent last :(
        Level.Instance.PlayParticleGlobally_ServerRpc("Confetti", _rb.position);
        Scoreboard.UpdateScore(killer, 1);
        transform.GetChild(1).GetComponent<NetworkObject>().Despawn();
        transform.GetChild(2).GetComponent<NetworkObject>().Despawn();
        NetworkObject.Despawn();
    }


    [ServerRpc]
    public void ApplyEffectServerRpc(int type, ServerRpcParams @params = default)
    {
        switch (type)
        {
            case 0:
                //TODO: learn how to make a custom hash, so we can send particles and materials through hashed ints for higher consistency.
                ApplySlowClientRpc(@params.Receive.SenderClientId);
                break;
            case 1:
                ApplyImmortalityClientRpc();
                break;
        }
        AddMaterialClientRpc(type);
    }
    [ServerRpc]
    public void RemoveEffectServerRpc(int type, int MaterialHashID = -1)
    {
        switch (type)
        {
            case 1:
                RemoveImmortalityClientRpc(MaterialHashID);
                break;
        }
    }


    [ClientRpc]
    private void ApplySlowClientRpc(ulong id)
    {
        previousAttacker = id;
        Acceleration *= 0.7f;
    }

    private int prvLayer;
    [ClientRpc]
    private void ApplyImmortalityClientRpc()
    {
        _rb.gameObject.layer = GameManager.ImmortalLayer;
    }
    
    [ClientRpc]
    private void RemoveImmortalityClientRpc(int hash)
    {
        _rb.gameObject.layer = IsOwner?GameManager.LocalLayer:GameManager.EnemyLayer;
        RemoveMaterial(hash);
    }

    [ClientRpc]
    private void AddMaterialClientRpc(int id)
    {
        Material mat = null;
        switch (id)
        {
            case 0:
                mat = new Material(ParticleManager.GlueBallMat);
    
                //Kill me :(
                //TODO: Generate a material that is actually the same as the one in game... I probably have to do some mapping shenanigans to actullay pull this off.
                mat.SetFloat(StaticUtilities.ColorID, Random.Range(0,1f));
                mat.SetInt(StaticUtilities.RandomTexID, Random.Range(0,4));
                mat.SetVector(StaticUtilities.RandomOffsetID, new Vector2(Random.Range(-0.25f,0.25f),Random.Range(-0.25f,0.25f)));
                break;
            case 1:
                mat = ParticleManager.ProtectMat;
                break;

        }

        int l = mr.materials.Length;
        Material[] mats = new Material[l + 1];
        for (int index = 0; index < l; index++)
        {
            mats[index] = mr.materials[index];
        }

        mats[l] = mat;
        mr.materials = mats;
    }

   

    private void RemoveMaterial(int hashID)
    {
        int l = mr.materials.Length;
        Material[] mats = new Material[l - 1];
        int m = 0;
        for (int index = 0; index < l; index++)
        {
            if (mats[index].GetHashCode() != hashID)
                mats[index] = mr.materials[m];
            m += 1;
        }

        mr.materials = mats;
    }

    [ClientRpc]
    public void FinalizeClientRpc()
    {
        print("Loaded fully.");


        Initialize();

        transform.GetChild(0).GetComponent<PositionConstraint>()
            .SetSource(0, new ConstraintSource { sourceTransform = transform.GetChild(1) });
    }
}