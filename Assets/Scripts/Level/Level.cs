using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

public class Level : NetworkBehaviour
{
    [SerializeField] private float bottomY;
    [SerializeField] private bool offMapKills;
    [field: SerializeField] public bool IsRandomSpawning { get; private set; }
    public static Level Instance { get; private set; }
    [field: SerializeField] public Transform [] PodiumPoints { get; private set; }
    [field: SerializeField] public Transform [] SpawnPoints { get; private set; }
    

    [SerializeField] private float distance;
    [SerializeField] private float travelTime;
    [SerializeField] private Transform coinStart;
    
    private Transform _coin;

    private static int spawnedIdx;
    public static Vector3 GetNextSpawnPoint() => Instance.SpawnPoints[spawnedIdx++ % Instance.SpawnPoints.Length].position;
    
    private void SpawnCoin()
    {
        Debug.Log("Spawning Map Coin");
        int r = Random.Range(0, 100);
        GameObject spawned;
        switch (r)
        {
            case 0:
                spawned = ParticleManager.SummonObjects["SpecialCoin"];
                break;
            case <= 20:
                spawned = ParticleManager.SummonObjects["WeaponCoin"];
                break;
            case <= 40:
                spawned = ParticleManager.SummonObjects["BallCoin"];
                break;
            case <= 60:
                spawned = ParticleManager.SummonObjects["AbilityCoin"];
                break;
           default:
                spawned = ParticleManager.SummonObjects["CosmeticCoin"];
                break;
        }
        _coin = Instantiate(spawned, coinStart.position, Quaternion.identity).transform;
        _coin.GetComponent<PositionConstraint>().constraintActive = false;
        _coin.GetComponent<NetworkObject>().Spawn(true);
        if (!IsLocalPlayer)
        {
            StartCoroutine(CoinTravel());
        }

    }

    //All levels drop coins from center...
    private readonly HashSet<ulong> _readyPlayers = new();
    private void Awake()
    {
        if(Instance != null) Destroy(Instance.gameObject);
        Instance = this;

        
        //When the player awakes... the server will   
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckGameStartServerRpc(ServerRpcParams @params = default)
    {
        _readyPlayers.Add(@params.Receive.SenderClientId);
        CheckStartGame();
    }

    void CheckStartGame()
    {
        print("Checking players connected: " + _readyPlayers.Count + " == " + NetworkManager.ConnectedClients.Count);
        if (_readyPlayers.Count == NetworkManager.ConnectedClients.Count)
        {
            StartGameClientRpc();
            SpawnCoin();
        }
        
    }





    private void Start()
    {
        
        print("Level awake");
        
        _readyPlayers.Add(OwnerClientId);
        CheckGameStartServerRpc();
        
        if(!IsOwner) return;
        print("SpawningCoin");
        
        
        
        NetworkManager.OnClientConnectedCallback += id =>
        {
            print("Player connected: " + id);
        };
       
        NetworkManager.OnClientDisconnectCallback += id =>
        {
            print("Player disconnected: " + id);
            _readyPlayers.Remove(id);
            CheckStartGame();
        };
    }

    void Update()
    {
        if (!BallPlayer.Alive) return;
        //Every player is responsible for checking if they've fallen off the map? Or should it be the server... probably the server...
        if (BallPlayer.LocalBallPlayer.BallY < bottomY)
        {
            BallPlayer.LocalBallPlayer.Respawn(offMapKills);
        }
    }

    private IEnumerator CoinTravel()
    {
        float curTravelTime = 0;
        float y = coinStart.position.y;
        while (curTravelTime < travelTime && _coin) //Logic to just check that the coin hasn't been destroyed, or reached it's destination
        {
            
            curTravelTime += Time.deltaTime;
            _coin.position = y * Vector3.up +(distance * (curTravelTime / travelTime) * Vector3.down);
            yield return null;
        }
    }
    
   

    [ClientRpc]
    private void StartGameClientRpc()
    {
        BallPlayer.LocalBallPlayer.Initialize();
        GameManager.GameStarted = true;
    }


#if UNITY_EDITOR
    [SerializeField] private bool display;
    private void OnDrawGizmos()
    {
        if (!display) return;
        Gizmos.color = Color.black;
        Gizmos.DrawCube(Vector3.up * bottomY, new Vector3(100000f,0.1f,100000f));
        Gizmos.color = Color.green;
        Gizmos.DrawRay(coinStart.position, Vector3.down * distance);
        Gizmos.DrawSphere(coinStart.position, 4);
    }
    #endif
}
