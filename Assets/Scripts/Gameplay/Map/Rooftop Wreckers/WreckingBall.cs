using UnityEngine;

namespace Gameplay.Map.Rooftop_Wreckers
{
    public class WreckingBall : MonoBehaviour
    {
        [SerializeField] private Rigidbody lastRB;
    
        private bool _hasHit = false;
    
        public void OnCollisionEnter(Collision other)
        {
            if (_hasHit)
            {
                return;
            }
        
            Debug.Log("WRECKING BALL HIT SOMETHING: " + other.gameObject.name);
        
            if (other.transform.parent.TryGetComponent(out Building building))
            {
                building.DestroyBuilding();
                _hasHit = true;
            }
        }
    
    }
}
