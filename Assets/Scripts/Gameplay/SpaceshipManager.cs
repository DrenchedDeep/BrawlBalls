using System;
using Managers.Network;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NetworkObject))]
public class SpaceshipManager : NetworkBehaviour
{
    public static SpaceshipManager Instance { get; private set; }

    [SerializeField] private GameObject spaceShipPrefab;
    [SerializeField] private bool drawLines;
    [SerializeField] private SpaceShipNav[] points;
    

    [Space]
    
    [SerializeField] private float spawnInterval;
    [SerializeField] private int spaceShipsToSpawn;

    private NetworkObject _spaceShip;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;
     
        for (int i = 0; i < spaceShipsToSpawn; i++)
        {
            float time = (i + 1) * spawnInterval;
            NetworkGameManager.Instance.AddTimedEvent(time, SpawnSpaceShip);
        } 
    }

    private void OnDrawGizmos()
    {
        if (drawLines)
        {
            foreach (SpaceShipNav nav in points)
            {
                Gizmos.DrawLine(nav.start.position, nav.end.position);
            }
        }
    }


    private void SpawnSpaceShip()
    {
        NetworkObject no = Instantiate(spaceShipPrefab, transform.position, Quaternion.identity)
            .GetComponent<NetworkObject>();

        if (no)
        {
            no.Spawn(true);
        }
        
        NetworkGameManager.Instance.SendMessage_ClientRpc("<color=#d4bb00>WRECKING BALL</color> has spawned", 2);


        _spaceShip = no;
        /*/
        _spaceShip = NetworkManager.SpawnManager.InstantiateAndSpawn(spaceShipPrefab.GetComponent<NetworkObject>(),
            NetworkManager.ServerClientId,
            true, false, false,
            transform.position, transform.rotation);
            /*/

        Debug.Log("spawn spaceship??");

        if (_spaceShip.gameObject.TryGetComponent(out WreckingBallManager wb))
        {
            wb.StartWrecking(points[Random.Range(0, points.Length)]);
        }
    }
}
