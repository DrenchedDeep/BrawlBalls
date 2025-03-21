using Managers;
using Managers.Network;
using Unity.Netcode;
using UnityEngine;
using Utilities.General;
using Random = UnityEngine.Random;

namespace Gameplay.Map
{
    public class Level : NetworkBehaviour
    {

        
        [Header("Coin")]
        [SerializeField] private Transform coinStart;
        
        [Header("Map")]
        [field: SerializeField] public Transform [] SpawnPoints { get; private set; }
        [SerializeField] private float bottomY;
        [SerializeField] private bool offMapKills;
        
        
        
      //  private static int SpawnedIdx { get; set; }
        private static Level Instance { get; set; }

        public static Vector3 GetNextSpawnPoint()
        {
            return Instance.SpawnPoints[Random.Range(0, Instance.SpawnPoints.Length)].position;
        }

        //All levels drop coins from center...
        private void Awake()
        {
            Instance = this;
        }

        //isServer was returning false in Awake(), and it works here cuz we have to wait for OnNetworkSpawn to know if we the server or not?
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
                        
            if (!IsServer)
            {
                enabled = false;
                return;
            }
            
            SpawnPoints.Shuffle();
            
           // Debug.Log("Spawn point shuffle: ");
            /*/
            foreach (var point in SpawnPoints)
                Debug.Log("SpawnPoint: " + point, point);
                /*/


            NetworkGameManager.Instance.AddTimedEvent(30, SpawnCoin);
            NetworkGameManager.Instance.AddTimedEvent(120, SpawnCoin);
            NetworkGameManager.Instance.AddTimedEvent(210, SpawnCoin);

        }
        
        private void SpawnCoin()
        {
            Debug.Log("Spawning Map Coin");
            int r = Random.Range(0, 100);
            string spawned;
            switch (r)
            {
                case <= 10:
                    spawned = "SpecialCoin";
                    break;
                case <= 35:
                    spawned = "WeaponCoin";
                    break;
                case <= 70:
                    spawned = "BallCoin";
                    break;
                default:
                    spawned = "AbilityCoin";
                    break;
            }

            //This is only running on server anyways.
            NetworkGameManager.Instance.SendMessage_ClientRpc("A <color=#d4bb00>coin</color> has spawned", 2);
            NetworkGameManager.Instance.SpawnObjectGlobally_ServerRpc(spawned, coinStart.position, Quaternion.identity);
        }

        void Update()
        {
            if (!offMapKills) return;
            //Check if any of the player have fallen off the map
            foreach (BallPlayer ball in BallHandler.ActiveBalls)
            {
                if (!ball.IsAlive)
                {
                    Debug.Log("ball isnt alive??");
                    return;
                }
                
                if (ball.transform.position.y < bottomY)
                {
                    Debug.Log("Player died from falling out of map, atteker is 0?");
                    ball.Die_Server(100);
                }
            }
        }

#if UNITY_EDITOR
        [SerializeField] private bool display;
        private void OnDrawGizmos()
        {
            if (!display) return;
            Gizmos.color = new Color(0,0,0,0.5f);
            Gizmos.DrawCube(Vector3.up * bottomY, new Vector3(100000f,0.1f,100000f));
            Gizmos.color = Color.yellow;
            Vector3 position = coinStart.position;
            Gizmos.DrawSphere(position, 4);
        }
#endif
    
    }
}
