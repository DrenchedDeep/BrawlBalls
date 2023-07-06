using System.Collections;
using UnityEngine;

namespace Gameplay.Object_Scripts
{
    public abstract class PlaceableObject : MonoBehaviour
    {
        [SerializeField] private bool canCollideWithSelf;
        protected Ball Owner;
        private bool canCollide;
        private static readonly WaitForSeconds delay = new WaitForSeconds(0.4f);
    
        public void Init(Ball myOwner)
        {
            Owner = myOwner;
            StartCoroutine(Spawn());
        }

        private IEnumerator Spawn()
        {
            yield return delay;
            canCollide = true;
        }


        //Can only collide with other players...
        private void OnTriggerEnter(Collider other)
        {
            
            if (!canCollide) return;
        
            Transform p = other.transform.parent;
            if (p == null) return;
            Ball b =p.GetComponent<Ball>();
            
            if (b == null || (!canCollideWithSelf && b == Owner)) return;
            OnHit(b);

        }

        protected abstract void OnHit(Ball hit);

    }
}
