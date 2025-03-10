using System;
using Unity.Netcode;
using UnityEngine;


public class WreckingBall : MonoBehaviour
{
    [SerializeField] private Rigidbody lastRB;
    
    
    public void OnCollisionEnter(Collision other)
    {
        Debug.Log("WRECKING BALL HIT SOMETHING: " + other.gameObject.name);
        
        if (other.transform.parent.TryGetComponent(out Building building))
        {
            building.DestroyBuilding();
        }
    }
    
}
