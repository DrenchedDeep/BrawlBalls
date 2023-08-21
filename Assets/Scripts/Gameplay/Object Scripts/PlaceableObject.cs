using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Gameplay.Object_Scripts
{
    public abstract class PlaceableObject : NetworkBehaviour
    {
        [SerializeField] private bool canCollideWithSelf;
        [SerializeField] private ESurfaceBindMethod bindToSurface;
        private bool canCollide;
        private static readonly WaitForSeconds Delay = new (0.4f);

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
                    Physics.Raycast(position, Vector3.down,  out RaycastHit h,3, GameManager.GroundLayers);
                    //transform.parent = h.transform; Parenting literally breaks fucking everything. Unity is garbage
                    transform.forward = h.normal;
                    Debug.Log(transform.position);
                    transform.position = h.point - h.normal*0.1f;
                    Debug.Log(transform.position);
                    break;
                case ESurfaceBindMethod.Sphere:
                    Collider[] cols = Physics.OverlapSphere(position, 3f, GameManager.GroundLayers);
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

            StartCoroutine(Spawn());
        }

        private IEnumerator Spawn()
        {
            if(UseDelay)
                yield return Delay;
            canCollide = true;
        }


        //Can only collide with other players...
        private void OnTriggerEnter(Collider other)
        {
            int layer = 1<<other.gameObject.layer;
            print(layer +", " + GameManager.PlayerLayers);
            if ((layer & GameManager.PlayerLayers) == 0) return; // If it's not a player, we can't hit it...
            
            Ball b = other.transform.parent.GetComponent<Ball>();
            
            if (b == null || (!canCollideWithSelf && b.OwnerClientId == OwnerClientId)) return;
            OnHit(b);

        }

        protected abstract void OnHit(Ball hit);

    }
}
