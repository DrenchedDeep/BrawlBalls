using System;
using Managers.Network;
using Unity.Netcode;
using UnityEngine;

public class IntroCinematic : MonoBehaviour
{
    public void OnEnable()
    {
        NetworkGameManager.Instance.ClientFinishedIntroCinematic_ServerRpc();

    }
}
