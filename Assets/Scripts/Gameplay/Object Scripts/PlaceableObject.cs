using UnityEngine;

namespace Gameplay.Object_Scripts
{
    public abstract class PlaceableObject : MonoBehaviour
    {
        [SerializeField] private bool canCollideWithSelf;
        protected Ball owner;
    
        public void Init(Ball myOwner)
        {
            owner = myOwner;
        }

        //Can only collide with other players...
        private void OnTriggerEnter(Collider other)
        {
            Ball b =other.transform.parent.GetComponent<Ball>();

            if (!canCollideWithSelf && b == owner) return;
        
            OnHit(b);

        }

        protected abstract void OnHit(Ball hit);

    }
}
