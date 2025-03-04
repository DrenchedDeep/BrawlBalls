using Gameplay.UI;
using Managers.Local;
using Managers.Network;
using RotaryHeart.Lib.PhysicsExtension;
using Stats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using Physics = UnityEngine.Physics;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class Ball : NetworkBehaviour
    {
        [SerializeField] private BallStats stats;
        private Rigidbody _rb;
        private MeshRenderer _mr;
        

        
        private Vector3 _previousPosition;
        private Vector3 _curPos;
        public readonly NetworkVariable<Vector2> MoveDirection = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public readonly NetworkVariable<Vector3> Foward = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        
        
        public float MaxSpeed => stats.MaxSpeed;
        public float MaxHealth => stats.MaxHealth;
        public float Mass => stats.Mass;
        public float Speed { get; private set; }
        public Vector3 Velocity { get; private set; }
        public float Acceleration { get; private set; }

        private void Start()
        {
            GetComponent<MeshRenderer>().material = stats.Material;
            GetComponent<MeshFilter>().mesh = stats.Mesh;
            
            _rb = GetComponentInParent<Rigidbody>();
            _mr = GetComponent<MeshRenderer>();
            
            Acceleration = stats.Acceleration;         

            _rb.useGravity = true;
            _rb.linearDamping = stats.Drag;
            _rb.angularDamping = stats.AngularDrag;
        }

        private void FixedUpdate()
        {
            //Runs only for other clients
            if (IsOwner)
            {
                HandleMovement();
                HandleDrag();
            }

            UpdateState();
        }



        private void UpdateState()
        {
            _previousPosition = _curPos;
            _curPos = _rb.position;
            Velocity = (_curPos - _previousPosition) / Time.fixedDeltaTime ;
            Speed = Velocity.magnitude;
        }

        public void ChangeVelocity(Vector3 dir, ForceMode forceMode = ForceMode.Impulse, bool stop = false)
        {
            if (!IsOwner) return;
            if (stop)
                _rb.linearVelocity = Vector3.zero;
            _rb.AddForce(dir, forceMode);
        }
        void HandleMovement()
        {
            Vector3 fwd = Foward.Value;
            Vector2 moveDir = MoveDirection.Value;
            
            _rb.AddForce(moveDir.y * Acceleration * fwd, ForceMode.Acceleration);
            _rb.AddForce(moveDir.x * Acceleration * Vector3.Cross( Vector3.up,fwd), ForceMode.Acceleration);
        
            Vector3 velocity = _rb.linearVelocity;

            float y = velocity.y;
            velocity.y = 0;
            
            _rb.linearVelocity = Vector3.ClampMagnitude(velocity, MaxSpeed) + Vector3.up * y; //maintain our Y
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

                Rigidbody n = h.rigidbody;
                if (n && n.TryGetComponent(out BallPlayer b))
                {
                    Debug.LogWarning("LANDED ON EM: " + b.name + ", " + name);
                    b.TakeDamage_ClientRpc(1000000, Vector3.zero, ulong.MaxValue);
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

        #region Effects MOVE THESE LATER
      [ServerRpc]
        public void ApplyEffect_ServerRpc(int type, ServerRpcParams @params = default)
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
            //_previousAttackerID = id;
            Debug.Log("Re-implement previous attacker logic.");
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
        #endregion
    }
}