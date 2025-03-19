using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    [SerializeField] private float lifeTime = 1;
    void Start()
    {
       Destroy(gameObject, lifeTime);
    }

}
