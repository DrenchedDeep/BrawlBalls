using System;
using Unity.Netcode;
using UnityEngine;


public class WreckingBall : MonoBehaviour
{
    [SerializeField] private Rigidbody lastRB;
    
    
    public void OnCollisionEnter(Collision other)
    {
        Debug.Log("WRECKING BALL HIT SOMETHING: " + other.gameObject.name);

        if (other.transform.root.TryGetComponent(out DestroyablePillar pillar))
        {
            pillar.OnWreckingBallHit(transform.position, lastRB.linearVelocity);
        }
    }
    
}
