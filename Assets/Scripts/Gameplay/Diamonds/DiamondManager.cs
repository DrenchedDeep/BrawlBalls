using Managers.Network;
using Unity.Netcode;
using UnityEngine;

public class DiamondManager : NetworkBehaviour
{
    [SerializeField] private int maxDiamondsAtOnce = 5;
    [SerializeField] private float diamondSpawnInterval = 10;
    [SerializeField] private DiamondSpawnPoint[] diamondSpawnPoints;


    public override void OnNetworkSpawn()
    {
        Debug.Log("diamond manager init");

        if (IsServer)
        {
            InvokeRepeating(nameof(TrySpawnDiamond), 0, diamondSpawnInterval);
        }
    }

    private void TrySpawnDiamond()
    {
        Debug.Log("try to spawn: " + GetCurrentDiamondsInLevel());
        if (GetCurrentDiamondsInLevel() >= maxDiamondsAtOnce)
        {
            return;
        }

        DiamondSpawnPoint spawn = GetSpawnPoint();
        NetworkGameManager.Instance.SpawnObjectGlobally_ServerRpc("Diamond", spawn.transform.position, spawn.transform.rotation);
    }

    private DiamondSpawnPoint GetSpawnPoint()
    {
        for (int i = 0; i < diamondSpawnPoints.Length; i++)
        {
            if (!diamondSpawnPoints[i].CanSpawnDiamond)
            {
                continue;
            }

            return diamondSpawnPoints[i];
        }

        return null;
    }

    private int GetCurrentDiamondsInLevel()
    {
        int currentDiamonds = 0;
        for (int i = 0; i < diamondSpawnPoints.Length; i++)
        {
            if (!diamondSpawnPoints[i].CanSpawnDiamond)
            {
                currentDiamonds++;
            }
        }

        return currentDiamonds;
    }
}
