using System.Collections.Generic;
using Gameplay;
using Gameplay.Map;
using Gameplay.UI;
using Managers.Local;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class BallHandler : NetworkBehaviour
    {
        private static int BallsSpawned { get; set; }
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
    
        //This function creates the players ball team composition
        public BallPlayer[] SpawnShowcaseBalls()
        {
            BallPlayer[] balls = new BallPlayer[3];
            
            int i = 0;
            foreach (PlayerBallInfo.BallStructure bs in PlayerBallInfo.Balls)
            {
                Vector3 p = Level.GetPodiumPoint(i);

                BallPlayer b = ResourceManager.CreateBallDisabled(bs.Ball, bs.Weapon, p, Quaternion.LookRotation(Vector3.up));
                
                balls[i++] = b;
            }
            return balls;
        }



        [ServerRpc(RequireOwnership = false)]
        public void SpawnBall_ServerRpc(string ball, string weapon, string ability, ServerRpcParams id =default)
        {
            Vector3 spawnPoint = Level.GetNextSpawnPoint();

            Debug.Log("Spawning at: " + spawnPoint + "Ball successfully spawned: " + id.Receive.SenderClientId);
            
            //Create the Ball Controller
            BallPlayer player = Instantiate(ResourceManager.Hull, spawnPoint, Quaternion.LookRotation(Vector3.up));
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
            
            player.Initialize_ServerRpc(ability);
        }
    }
}
