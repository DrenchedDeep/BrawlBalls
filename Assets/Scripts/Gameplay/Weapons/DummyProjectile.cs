using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DummyProjectile : MonoBehaviour
{
    private Rigidbody _rigidbody;
    

    public void Init(Vector3 velocity, float lifetime)
    {
        _rigidbody = GetComponent<Rigidbody>();
 
        _rigidbody.linearVelocity = velocity;
        Destroy(gameObject, lifetime);
    }
}
