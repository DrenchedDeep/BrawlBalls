using UnityEngine;

public class NameTag : MonoBehaviour
{
    
    
    void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

    }
}
