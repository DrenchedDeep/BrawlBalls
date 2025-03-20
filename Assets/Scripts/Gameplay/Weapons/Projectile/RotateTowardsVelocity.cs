using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RotateTowardsVelocity : MonoBehaviour
{
   private Rigidbody _rigidbody;


   private void Awake()
   {
      _rigidbody = GetComponent<Rigidbody>();
   }

   private void FixedUpdate()
   {
      transform.rotation = Quaternion.LookRotation(_rigidbody.linearVelocity);
   }
}
