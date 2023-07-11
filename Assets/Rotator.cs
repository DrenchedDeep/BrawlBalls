using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 speed;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.eulerAngles += speed;
    }
}
