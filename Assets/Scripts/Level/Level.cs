
using System;
using System.Collections;
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

    [SerializeField] private float distance;
    [SerializeField] private float travelTime;
    [SerializeField] private Transform coinStart;
    
    private Transform coin;

    private void SpawnCoin()
    {
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
        coin = Instantiate(spawned, coinStart.position, Quaternion.identity).transform;
        coin.GetComponent<PositionConstraint>().constraintActive = false;
        if (!IsLocalPlayer)
        {
            StartCoroutine(CoinTravel());
        }

    }

    //All levels drop coins from center...
    
    

    private void Awake()
    {
        if(Instance) Destroy(gameObject);
        Instance = this;
       
    }

    private void Start()
    {
        SpawnCoin();
    }


    void Update()
    {
        if (!IsOwner) return;
        //Every player is responsible for checking if they've fallen off the map? Or should it be the server... probably the server...
        if (Player.LocalPlayer.BallY < bottomY)
        {
            if (offMapKills)
            {
                //Player.LocalPlayer.TakeDamage(100000);
            }
            else
            {
                Player.LocalPlayer.Respawn(false);
            }
        }
    }

    private IEnumerator CoinTravel()
    {
        float curTravelTime = 0;
        float y = coinStart.position.y;
        while (curTravelTime < travelTime && coin) //Logic to just check that the coin hasn't been destroyed, or reached it's destination
        {
            
            curTravelTime += Time.deltaTime;
            coin.position = y * Vector3.up +(distance * (curTravelTime / travelTime) * Vector3.down);
            yield return null;
        }
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
