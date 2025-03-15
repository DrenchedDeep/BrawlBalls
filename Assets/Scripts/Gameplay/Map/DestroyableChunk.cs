using UnityEngine;

namespace Gameplay.Map
{
    public class DestroyableChunk : MonoBehaviour
    {
        private bool _canDestroy = false;
        public void SetCanDestroy(bool canDestroy) => _canDestroy = canDestroy;
    
        public void OnCollisionEnter(Collision other)
        {
            if (_canDestroy)
            {
                //  Debug.Log("CHUNK HIT THING!");
                //     Destroy(gameObject);
            }
        }
    }
}
