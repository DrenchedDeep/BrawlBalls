using System;
using RotaryHeart.Lib.PhysicsExtension;
using Unity.Netcode;
using UnityEngine;
using Physics = UnityEngine.Physics;


[RequireComponent(typeof(NetworkObject))]
public class DestroyablePillar : NetworkBehaviour
{
    public NetworkVariable<bool> IsDestroyed { get; private set; } = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Rigidbody[] _rigidbodies;

    private void Start()
    {
        _rigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in _rigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    //can only be set on the server, when its replicated down the physics will be enabled
    public void OnWreckingBallHit(Vector3 hitLocation, Vector3 hitVelocity)
    {
        DestroyPillar_ClientRpc(hitLocation, hitVelocity);
    }

    [ClientRpc]
    void DestroyPillar_ClientRpc(Vector3 hitLocation, Vector3 hitVelocity)
    {
        RaycastHit[] results = Physics.SphereCastAll(hitLocation, hitVelocity.magnitude, Vector3.up);

        foreach (RaycastHit hit in results)
        {
            if (hit.collider.gameObject.TryGetComponent(out DestroyableChunk chunk))
            {
                if (chunk.TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = false;
                }
            } 
        }
    }

    public override void OnNetworkSpawn()
    {
        IsDestroyed.Value = false;
        IsDestroyed.OnValueChanged += OnIsDestroyedChanged;
    }


    public void OnIsDestroyedChanged(bool old, bool current)
    {
        foreach (Rigidbody rb in _rigidbodies)
        {
            rb.isKinematic = false;
            if (rb.gameObject.TryGetComponent(out DestroyableChunk chunk))
            {
                chunk.SetCanDestroy(true);
            }
        }
    }
}
