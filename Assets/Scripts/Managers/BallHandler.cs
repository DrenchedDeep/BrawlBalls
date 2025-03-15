using System.Collections.Generic;
using Gameplay;
using Gameplay.Map;
using Managers.Local;
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
        public void SpawnBall_ServerRpc(string ball, string weapon, string ability, ServerRpcParams id =default)
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
            NetworkObject pl = player.GetComponent<NetworkObject>();
            pl.SpawnAsPlayerObject(id.Receive.SenderClientId, true);
            
            obs[0].TrySetParent(pl);
            obs[1].TrySetParent(pl);

            ActiveBalls.Add(player);

            player.OnDestroyed += (_) =>
            {
                ActiveBalls.Remove(player);
            };
            
            player.Initialize(ability, false);
        }

        #if UNITY_EDITOR
        public void SpawnBall_Offline(string ball, string weapon, string ability)
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
            
            player.Initialize(ability, true);
        }
        #endif
    }
}
