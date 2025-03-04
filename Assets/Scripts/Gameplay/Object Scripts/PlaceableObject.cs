using Cysharp.Threading.Tasks;
using Managers;
using Managers.Local;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Object_Scripts
{
    public abstract class PlaceableObject : NetworkBehaviour
    {
        [SerializeField] private bool canCollideWithSelf;
        [SerializeField] private ESurfaceBindMethod bindToSurface;

        private enum ESurfaceBindMethod
        {
            None,
            Down,
            Sphere
        }

        protected virtual bool UseDelay => true;
        private void Awake()
        {

            print("PlacableObject deployed.");
            if (!IsOwnedByServer)
            {
                enabled = false;
                return;
            }
            print("Is owner of said object");
            
            Vector3 position = transform.position;
            switch (bindToSurface)
            {
                case ESurfaceBindMethod.Down:
                    Physics.Raycast(position, Vector3.down,  out RaycastHit h,3, StaticUtilities.GroundLayers);
                    //transform.parent = h.transform; Parenting literally breaks fucking everything. Unity is garbage
                    transform.forward = h.normal;
                    Debug.Log(transform.position);
                    transform.position = h.point - h.normal*0.1f;
                    Debug.Log(transform.position);
                    break;
                case ESurfaceBindMethod.Sphere:
                    Collider[] cols = Physics.OverlapSphere(position, 3f, StaticUtilities.GroundLayers);
                    float dist = float.MaxValue;
                    Vector3 best = Vector3.zero;
                   
                    foreach (Collider c in cols)
                    {
                        print("Checking objects...");
                        Vector3 d = c.ClosestPoint(position);
                        float m = (d - position).sqrMagnitude;
                        print(c.name + "Comp: " + dist +" , " + m);
                        if (dist > m)
                        {
                            best = d;
                            dist = m;
                        }
                    }

                    Vector3 normal = Vector3.Cross(best - position, position - best).normalized;
                    Debug.DrawRay(best, normal, Color.cyan, 3, false);
                    
                    print(best);
                    
                    transform.forward = normal;
                    transform.position = best - normal*0.1f;
                    
                    break;
            }
            
            Debug.LogWarning("Spawned a portal? There may be some incomplete code here?");
            _ =Spawn();
        }

        private async UniTask Spawn()
        {
            if(UseDelay) await UniTask.Delay(400);
        }


        //Can only collide with other players...
        private void OnTriggerEnter(Collider other)
        {
            int layer = 1<<other.gameObject.layer;
            print(layer +", " + StaticUtilities.PlayerLayers);
            if ((layer & StaticUtilities.PlayerLayers) == 0) return; // If it's not a player, we can't hit it...
            
            BallPlayer b = other.attachedRigidbody?.GetComponent<BallPlayer>();
            
            if (b == null || (!canCollideWithSelf && b.OwnerClientId == OwnerClientId)) return;
            OnHit(b);

        }

        protected abstract void OnHit(BallPlayer hit);

    }
}
