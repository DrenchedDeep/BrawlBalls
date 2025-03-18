using UnityEngine;
namespace Gameplay.Map.Utility
{
    [ExecuteInEditMode]
    public class RingMaker : MonoBehaviour
    {
        [SerializeField] private float totalDegrees = 360f;
        [SerializeField] private float distance = 200f;
        private void OnDrawGizmosSelected()
        {
            totalDegrees %= 361;
            float degsPerTick  = (totalDegrees / transform.childCount) ;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform target = transform.GetChild(i);
                float degs = degsPerTick * i ;
                float rads = Mathf.Deg2Rad * degs;
                float x = Mathf.Cos(rads) * distance;
                float z = Mathf.Sin(rads) * distance;
                target.SetLocalPositionAndRotation(new Vector3(x,0,z), Quaternion.Euler(0,180-degs,0));
            }
        }
    }
}