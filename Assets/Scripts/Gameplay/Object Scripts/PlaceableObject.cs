using System.Collections;
using UnityEngine;

namespace Gameplay.Object_Scripts
{
    public abstract class PlaceableObject : MonoBehaviour
    {
        [SerializeField] private bool canCollideWithSelf;
        [SerializeField] private bool bindToSurface;
        protected Ball Owner;
        private bool canCollide;
        private static readonly WaitForSeconds Delay = new (0.4f);
    
        public void Init(Ball myOwner)
        {
            Owner = myOwner;
        }

        private void Awake()
        {
            if (bindToSurface)
            {
                Physics.Raycast(transform.position, Vector3.down,  out RaycastHit h,3, 1);
                //transform.parent = h.transform; Parenting literally breaks fucking everything. Unity is garbage
                transform.forward = h.normal;
            }

            StartCoroutine(Spawn());
        }

        private IEnumerator Spawn()
        {
            yield return Delay;
            canCollide = true;
        }


        //Can only collide with other players...
        private void OnTriggerEnter(Collider other)
        {
            Transform p = other.transform.parent;
            if (p == null) return;
            Ball b =p.GetComponent<Ball>();
            
            if (b == null || (!canCollideWithSelf && b == Owner)) return;
            OnHit(b);

        }

        protected abstract void OnHit(Ball hit);

    }
}
