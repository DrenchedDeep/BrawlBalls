using System;
using Managers.Network;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct SpaceShipNav
{
    public Transform start;
    public Transform end;

    public SpaceShipNav(Transform start, Transform end)
    {
        this.start = start;
        this.end = end;
    }
}

[RequireComponent(typeof(NetworkObject))]
public class WreckingBallManager : NetworkBehaviour
{ 
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private Transform endTransform;
    
    
    private Rigidbody _rigidbody;

    private Vector3 _start;
    private Vector3 _end;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    //everybody simulate the spaceship movement... straightforward and this will get rid of lag + corrections causing the wrecking ball to clip through stuff on the client(s).
    private void FixedUpdate()
    {
        _rigidbody.AddForce((_end -_start).normalized * moveSpeed, ForceMode.VelocityChange);
        
        float delta  = Vector3.Distance(transform.position, _end);
        
        if (delta <= 200)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    public void StartWrecking(SpaceShipNav nav)
    {
     //   _spaceShipNav = nav;
    //    transform.position = _spaceShipNav.start.position;
     //   transform.rotation = _spaceShipNav.start.rotation;
        StartWrecking_ClientRpc(nav.start.position, nav.end.position);
    }

    [ClientRpc]
    public void StartWrecking_ClientRpc(Vector3 start, Vector3 end)
    {
        _start = start;
        _end = end;
        transform.position = start;
    }
}

