using System;
using System.Collections.Generic;
using System.Linq;
using Managers.Local;
using RotaryHeart.Lib.PhysicsExtension;
using Stats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Physics = UnityEngine.Physics;
using Random = UnityEngine.Random;

namespace Gameplay.Balls
{
    public class Ball : NetworkBehaviour
    {
        [SerializeField] private BallStats stats;
        [SerializeField] private PlayerInput localPlayerInputComponent;
        [SerializeField] private InputActionReference jump;

        [Space] 
        
        [Header("JUMP")] 
        [SerializeField] private bool scaleByMass;
        [SerializeField] protected float defaultJumpForce = 1500;
        
        public BallStats Stats => stats;
        
        private Rigidbody _rb;
        private MeshRenderer _mr;
        private Vector3 _previousPosition;
        private Vector3 _curPos;
        private bool _isGrounded;
        public bool IsGrounded => _isGrounded;

        public Rigidbody RigidBody => _rb;

        public event Action OnGroundStateChanged;


        public NetworkVariable<bool> IsProtected { get; set; } = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> IsGlued { get; set; } = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        
       // [NonSerialized] public Vector2 MoveDirection;
        public readonly NetworkVariable<Vector2> MoveDirection = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        //[NonSerialized] public Vector3 Foward;
        public readonly NetworkVariable<Vector3> Foward = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
       
       public float Speed { get; private set; }
       public Vector3 Velocity { get; private set; }
       public float Acceleration { get; private set; }
       
       

       protected virtual void Start()
       {
           _rb = GetComponentInParent<Rigidbody>();
           _mr = GetComponent<MeshRenderer>();
            
           Acceleration = stats.Acceleration;         

           _rb.useGravity = true;
           _rb.angularDamping = stats.AngularDrag;
           
           if (!NetworkManager)
           {
               SetupJoystick();
           }
       }
       
       public override void OnNetworkSpawn()
       {
           IsProtected.OnValueChanged += OnIsProtectedChanged;
           IsGlued.OnValueChanged += OnIsGluedChanged;
       }
       

       [ServerRpc(RequireOwnership = false)]
       public void AddImpulse_ServerRpc(Vector3 velocity)
       {
           AddImpulse_ClientRpc(velocity);
       }
       
       
       [ClientRpc(RequireOwnership = false)]
       public void AddImpulse_ClientRpc(Vector3 velocity)
       {
           _rb.AddForce(velocity, ForceMode.Impulse);
       }

       private void SetupJoystick()
       {
        //   LocalPlayerController.LocalBallPlayer.SwapJoySticks(fullJoyStick);
       }


       private void FixedUpdate()
       {
           //if (_rb.isKinematic) return;
           #if UNITY_EDITOR
           if (IsOwner || !NetworkManager.Singleton)
           {
               HandleMovement();
               HandleGround();
           }
           #else
           if (IsOwner)
           {
               HandleMovement();
               HandleGround();
           }
           #endif
  

           UpdateState();
       }
       
       protected virtual bool CanJump()
       {
           return _isGrounded;
       }


       public virtual void Jump(bool checkForCanJump = true)
       {
           if (checkForCanJump && !CanJump())
           {
               return;
           }
           
           RigidBody.AddForce(defaultJumpForce * Vector3.up, ForceMode.Impulse);
       }
       
       //CALLED FROM BALL PLAYER: CLIENT ONLY FUNCTION
       public virtual void Init(BallPlayer ballPlayer)
       {
           SetupJoystick();
           localPlayerInputComponent = ballPlayer.Owner.PlayerInput;
           localPlayerInputComponent.actions[jump.name].performed += _ => Jump();

       }



       public virtual void UpdateState()
       {
           _previousPosition = _curPos;
           _curPos = _rb.position;
           Velocity = (_curPos - _previousPosition) / Time.fixedDeltaTime ;
           Speed = Velocity.magnitude;
       }

       public virtual void ChangeVelocity(Vector3 dir, ForceMode forceMode = ForceMode.Impulse, bool stop = false)
       {
           if (!IsOwner)
           {
               Debug.Log("is not owner");
               return;
               
           }

           if (stop)
           {
               Debug.Log("Stop");
               _rb.linearVelocity = Vector3.zero;
           }

           _rb.AddForce(dir, forceMode);
       }
        
       public virtual void HandleMovement()
       {
           Vector3 fwd = Foward.Value;
           //Vector3 fwd = Foward.Value;
           Vector2 moveDir = MoveDirection.Value;
           //Vector2 moveDir = MoveDirection.Value;
           
           _rb.AddForce(moveDir.y * Acceleration * fwd, ForceMode.Acceleration);
           _rb.AddForce(moveDir.x * Acceleration * Vector3.Cross( Vector3.up,fwd), ForceMode.Acceleration);
        
           Vector3 velocity = _rb.linearVelocity;

           float y = velocity.y;
           velocity.y = 0;
            
           _rb.linearVelocity = Vector3.ClampMagnitude(velocity, Stats.MaxSpeed) + Vector3.up * y; //maintain our Y
       }
        
        
       private void HandleGround()
       {
           bool hit = Physics.SphereCast(_rb.position,stats.FootRadius, Vector3.down, out RaycastHit h, stats.FootRange, StaticUtilities.GroundLayers, QueryTriggerInteraction.Ignore);

           if (hit != _isGrounded)
           {
               _isGrounded = hit;
               OnGroundStateChanged?.Invoke();
           }
           
#if UNITY_EDITOR
           DebugExtensions.DebugSphereCast(_rb.position, Vector3.down,  stats.FootRadius, hit?Color.red:Color.green, stats.FootRange, 0, CastDrawType.Complete, PreviewCondition.Editor,true);
#endif
           //Handle squishing
           if (hit)
           {
               if ((1 << h.transform.gameObject.layer & StaticUtilities.GroundLayers) != 0)
               {
                   return;
               }

               Rigidbody n = h.rigidbody;
               if (n && n.TryGetComponent(out BallPlayer b))
               {
                   Debug.LogWarning("LANDED ON EM: " + b.name + ", " + name); 
                   b.TakeDamage_ServerRpc(new DamageProperties(1000000, Vector3.zero, ulong.MaxValue));
               }
           }
       }

       #region Effects MOVE THESE LATER
       
       /*/
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
       /*/
       
       
       /*/
       
       [ServerRpc]
       public void RemoveEffect_ServerRpc(int type, int materialHashID = -1)
       {
           switch (type)
           {
               case 1:
                   RemoveImmortality_ClientRpc(materialHashID);
                   break;
           }
       }
       /*/

       /*/
        [ClientRpc]
        private void ApplyImmortalityClientRpc()
        {
            _rb.gameObject.layer = StaticUtilities.ImmortalLayerLiteral;
        }
    
        [ClientRpc]
        private void RemoveImmortality_ClientRpc(int hash)
        {
            _rb.gameObject.layer = IsOwner?StaticUtilities.LocalBallLayerLiteral:StaticUtilities.EnemyLayerLiteral;
            RemoveMaterial(hash);
        }
        /*/

       [ClientRpc]
       private void ApplySlowClientRpc(ulong id)
       {
           //_previousAttackerID = id;
           Debug.Log("Re-implement previous attacker logic.");
           Acceleration *= 0.7f;
       }
       
       private void OnIsProtectedChanged(bool old, bool current)
       {
           if (current)
           {
               _rb.gameObject.layer = StaticUtilities.ImmortalLayerLiteral;
               AddMaterial(1);
           }
           else
           {
               RemoveImmortality();
           }
       }

       private void OnIsGluedChanged(bool old, bool current)
       {
           if (current)
           {
               AddMaterial(0);
           }
       }

       
       
        private void RemoveImmortality()
        {
            _rb.gameObject.layer = IsOwner?StaticUtilities.LocalBallLayerLiteral:StaticUtilities.EnemyLayerLiteral;
            RemoveMaterial("Protect");
        }
        
        private void AddMaterial(int id)
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
        
        private void RemoveMaterial(string inName)
        {
            List<Material> materials = _mr.materials.ToList();

            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].name.Contains(inName))
                {
                    materials.RemoveAt(i);
                }
            }
                        
            foreach (Material mat in materials)
            {
                Debug.Log("Material is: " + mat.name);
            }

            
            _mr.materials = materials.ToArray();

            
            /*/
            int l = _mr.materials.Length;
            Material[] mats = new Material[l]; //-1 so we don't do unneeded ones
            int m = 0;
            for (int index = 0; index < l; index++)
            {
                if (_mr.materials[index].name != matName)
                {
                    mats[index] = _mr.materials[index];
                }
            }
            _mr.materials = mats;
            /*/
        }
        
        /*/
        
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
        /*/
        #endregion
    }
}