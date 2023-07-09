using UnityEngine;

public class Collectable : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Object collected");
        //Play effect...
        Destroy(gameObject);
    }
}
