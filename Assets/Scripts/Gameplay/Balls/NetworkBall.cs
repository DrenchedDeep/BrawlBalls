using Managers;
using RotaryHeart.Lib.PhysicsExtension;
using Stats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using Physics = UnityEngine.Physics;
using Random = UnityEngine.Random;

namespace Gameplay.Balls
{
    public class NetworkBall : NetworkBehaviour, IDamageAble
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

        private ulong _previousAttackerID;

        private MeshRenderer _mr;

        private Vector3 _previousPosition;
        private Vector3 _curPos;
        
        public float Speed { get; private set; }
        public Vector3 Velocity { get; private set; }
        
        public float Acceleration { get; private set; }
        
        private void Initialize()
        {
            
            Transform t = transform.GetChild(1);
            _rb = t.GetComponent<Rigidbody>();
            _mr = t.GetComponent<MeshRenderer>();
            
            Weapon w = transform.GetChild(2).GetComponent<Weapon>();
            
            Acceleration = stats.Acceleration;         
            _currentHealth = stats.MaxHealth;

            _rb.useGravity = true;
            _rb.linearDamping = stats.Drag;
            _rb.angularDamping = stats.AngularDrag;
            _rb.mass = stats.Mass + w.Mass;
   
            
            print("HP: " + _currentHealth);
            _mr.sharedMaterial = BaseMaterial; //TODO: Should this be shared material? Do we edit it anywhere?
            t.GetComponent<MeshFilter>().sharedMesh = BaseMesh;
            NetworkObject.enabled = true;
            _rb.gameObject.layer = IsOwner?StaticUtilities.LocalBallLayerLiteral:StaticUtilities.EnemyLayerLiteral;
            
            if (IsOwner)
            {
                print("I'm owned by local");
                BallPlayer.LocalBallPlayer.SetBall(this);
                BallPlayer.LocalBallPlayer.SetWeapon(w);
            }
            
            enabled = true;
        }

        private void FixedUpdate()
        {
            //Runs only for other clients
            if (!IsOwner) return;
            HandleDrag();
            UpdateState();
        }

        private void Update()
        {
            //Runs only on client
            if (IsOwner) return;
            UpdateState();

        }

        private void UpdateState()
        {
            _previousPosition = _curPos;
            _curPos = _rb.position;
            Velocity = _curPos - _previousPosition;
            Speed = Velocity.magnitude * Time.deltaTime;
        }

        public void ChangeVelocity(Vector3 dir, ForceMode forceMode = ForceMode.Impulse, bool stop = false)
        {
            if (!IsOwner) return;
            if (stop)
                _rb.linearVelocity = Vector3.zero;
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
                _previousAttackerID = attacker;
                _ = MessageHandler.Instance.HandleScreenMessage("Died to: <color=#ff000>" + attacker + "</color>", 3f);
                DieServerRpc(attacker);
                BallPlayer.LocalBallPlayer.Respawn(false);
                //return;
            }

            _rb.AddForce(direction, ForceMode.Impulse);
        }


        private void HandleDrag()
        {
            bool hit = Physics.SphereCast(_rb.position,0.2f, Vector3.down, out RaycastHit h, 1.5f, StaticUtilities.GroundLayers);
            #if UNITY_EDITOR
            DebugExtensions.DebugSphereCast(_rb.position, Vector3.down,  0.2f, hit?Color.red:Color.green, 1.5f, 0, CastDrawType.Complete, PreviewCondition.Editor,true);
            #endif
            //Handle squishing
            if (hit)
            {
                if ((1 << h.transform.gameObject.layer & StaticUtilities.GroundLayers) != 0)
                {
                    _rb.linearDamping = stats.Drag;
                    return;
                }

                Transform n = h.transform.parent;
                if (n && n.TryGetComponent(out NetworkBall b))
                {
                    Debug.LogWarning("LANDED ON EM: " + b.name + ", " + name);
                    b.TakeDamageClientRpc(1000000, Vector3.zero, ulong.MaxValue);
                }
                else
                {
                    _rb.linearDamping = 0;
                }
            }
            else
            {
                _rb.linearDamping = 0;
            }
        }


        [ServerRpc]
        private void DieServerRpc(ulong killer)
        {
            //previousAttacker.AwardKill();
            //Instantiate(onDestroy,transform.position,previousAttacker.transform.rotation);
            //Because it needs to be parent last :(
            Level.Level.Instance.PlayParticleGlobally_ServerRpc("Confetti", _rb.position);
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
        public void RemoveEffectServerRpc(int type, int materialHashID = -1)
        {
            switch (type)
            {
                case 1:
                    RemoveImmortalityClientRpc(materialHashID);
                    break;
            }
        }


        [ClientRpc]
        private void ApplySlowClientRpc(ulong id)
        {
            _previousAttackerID = id;
            Acceleration *= 0.7f;
        }

        [ClientRpc]
        private void ApplyImmortalityClientRpc()
        {
            _rb.gameObject.layer = StaticUtilities.ImmortalLayerLiteral;
        }
    
        [ClientRpc]
        private void RemoveImmortalityClientRpc(int hash)
        {
            _rb.gameObject.layer = IsOwner?StaticUtilities.LocalBallLayerLiteral:StaticUtilities.EnemyLayerLiteral;
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

            int l = _mr.materials.Length;
            Material[] mats = new Material[l + 1];
            for (int index = 0; index < l; index++)
            {
                mats[index] = _mr.materials[index];
            }

            mats[l] = mat;
            _mr.materials = mats;
        }

   

        private void RemoveMaterial(int hashID)
        {
            int l = _mr.materials.Length;
            Material[] mats = new Material[l - 1];
            int m = 0;
            for (int index = 0; index < l; index++)
            {
                if (mats[index].GetHashCode() != hashID)
                    mats[index] = _mr.materials[m];
                m += 1;
            }

            _mr.materials = mats;
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
}