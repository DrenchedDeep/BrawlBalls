using UnityEngine;

#if UNITY_EDITOR
namespace MarkedForDeath
{
    [ExecuteAlways]
#endif
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private Vector3 speed;
        [SerializeField] private Vector3 startingRotation;

        private void Start()
        {
            transform.eulerAngles = startingRotation;
            Debug.LogWarning("The rotator is old and depreciated. Replace with new Utility Rotator and Hover Utilities.", gameObject);
        }

#if UNITY_EDITOR
        [SerializeField] private bool runDuringEditor;
#endif
        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            if (!runDuringEditor) return;
#endif
        
            transform.eulerAngles += speed * Time.deltaTime;
        }
    }
}
