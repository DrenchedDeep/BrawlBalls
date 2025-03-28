using System.Collections.Generic;
using Gameplay;
using Gameplay.Map;
using Managers.Local;
using Managers.Network;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class BallHandler : NetworkBehaviour
    {
        public static BallHandler Instance { get; private set; }

        public static readonly List<BallPlayer> ActiveBalls = new();
        


        // Start is called before the first frame update
        void Awake()
        {
            ActiveBalls.Clear();
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    




        [ServerRpc(RequireOwnership = false)]
        public void SpawnBall_ServerRpc(string ball, string weapon, string ability, int playerIndex, ServerRpcParams id =default)
        {
            Vector3 spawnPoint = Level.GetNextSpawnPoint();

            Debug.Log("Spawning at: " + spawnPoint + "Ball successfully spawned: " + id.Receive.SenderClientId);
            
            //Create the Ball Controller
            BallPlayer player = Instantiate(ResourceManager.Instance.Hull, spawnPoint, Quaternion.LookRotation(Vector3.up));
            Transform cachedTransform = player.transform;
            
            NetworkObject[] obs =
            {
                Instantiate(ResourceManager.Balls[ball], cachedTransform).GetComponent<NetworkObject>(),
                Instantiate(ResourceManager.Weapons[weapon], cachedTransform).GetComponent<NetworkObject>(),
            };
            
            foreach (NetworkObject ngo in obs)
            {
                ngo.SpawnWithOwnership( id.Receive.SenderClientId , true);
            }

            player.transform.position = spawnPoint;
            NetworkObject pl = player.GetComponent<NetworkObject>();
            pl.SpawnWithOwnership(id.Receive.SenderClientId, true);
            Physics.SyncTransforms();
            
            obs[0].TrySetParent(pl);
            obs[1].TrySetParent(pl);

            ActiveBalls.Add(player);

            player.OnDestroyed += (_) =>
            {
                ActiveBalls.Remove(player);
            };
            
            NetworkGameManager.Instance.OnBallSpawned();
            
            player.Initialize(ability, playerIndex);
        }
        

        #if UNITY_EDITOR
        public void SpawnBall_Offline(string ball, string weapon, string ability, int playerIndex)
        {
            Vector3 spawnPoint = Level.GetNextSpawnPoint();
            Debug.Log(spawnPoint);
            //Create the Ball Controller
            BallPlayer player = Instantiate(ResourceManager.Instance.Hull, spawnPoint, Quaternion.LookRotation(Vector3.up));
            Transform cachedTransform = player.transform;

            Instantiate(ResourceManager.Balls[ball], cachedTransform);
            Instantiate(ResourceManager.Weapons[weapon], cachedTransform);

            ActiveBalls.Add(player);

            player.OnDestroyed += (_) =>
            {
                ActiveBalls.Remove(player);
            };
            
            player.Initialize_Offline(ability, playerIndex);
        }
        #endif
    }
}
