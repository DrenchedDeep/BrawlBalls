using Gameplay.Balls;
using UI;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class BallHandler : NetworkBehaviour
    {
        private static int BallsSpawned { get; set; }
        public static BallHandler Instance { get; private set; }
    

        // Start is called before the first frame update
        void Awake()
        {
            Instance = this;
        }
    
        public NetworkBall[] SpawnBalls()
        {
            NetworkBall[] balls = new NetworkBall[3];
            int i = 0;
            foreach (PlayerBallInfo.BallStructure bs in PlayerBallInfo.Balls)
            {
                Vector3 p = Level.Level.Instance.PodiumPoints[i].position;
                NetworkBall b = Instantiate(GameManager.Balls[bs.Ball], p, Quaternion.LookRotation(Vector3.up));
                NetworkObject g = Instantiate(GameManager.Hull, b.transform);
                g.GetComponent<MeshRenderer>().material = b.BaseMaterial;
                g.GetComponent<MeshFilter>().mesh = b.BaseMesh;
                Instantiate(GameManager.Weapons[bs.Weapon],b.transform);
                b.SetAbility(GameManager.Abilities[bs.Ability]);
                balls[i++] = b;
            }
            return balls;
            /*
        foreach (PlayerBallInfo.BallStructure b in PlayerBallInfo.Balls)
        {
            print("Attempting to spawn ball.." + b.Ball);
            SpawnBallServerRpc(b.Ball, b.Weapon);
        }*/
            //The ability can be saved entirely locally.
        }



        [ServerRpc(RequireOwnership = false)]
        public void SpawnBallServerRpc(string ball, string weapon, ServerRpcParams id =default)
        {
        
            print("Ball successfully spawned: " + id.Receive.SenderClientId);
            NetworkBall b = Instantiate(GameManager.Balls[ball]);
            NetworkObject nb = b.GetComponent<NetworkObject>();
            nb.SpawnAsPlayerObject(id.Receive.SenderClientId, true);
            Vector3 spawnPoint = Level.Level.GetNextSpawnPoint();
            Debug.Log("Spawning at: " + spawnPoint);
            NetworkObject hull = Instantiate(GameManager.Hull, spawnPoint, Quaternion.LookRotation(Vector3.zero));
            hull.SpawnWithOwnership(id.Receive.SenderClientId, true);
            hull.TrySetParent(nb);
            hull.transform.position = spawnPoint;
            //hull.ChangeOwnership(id.Receive.SenderClientId); //?
        
            Weapon w = Instantiate(GameManager.Weapons[weapon]);
            NetworkObject nw = w.GetComponent<NetworkObject>();
            nw.SpawnWithOwnership(id.Receive.SenderClientId, true);
        
            nw.TrySetParent(nb);

            b.FinalizeClientRpc();
        
            //b.SetAbility(GameManager.Abilities[ability]);
        
            BallsSpawned += 1;
            print("Clients spawned: " + BallsSpawned);
        }
    }
}
