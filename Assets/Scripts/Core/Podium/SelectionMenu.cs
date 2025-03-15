using Managers;
using Managers.Local;
using Managers.Network;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Core.Podium
{
    public class SelectionMenu : MonoBehaviour
    {

        [SerializeField] private CinemachineCamera cam;
        [SerializeField] private PodiumController podiumController;
        
        public static SelectionMenu Instance { get; private set; }


        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            podiumController.onForwardSelected.AddListener(TrySpawnSelectedBall);
            
            BeginDisplaying();
        }

        private void Start()
        {
            if (NetworkGameManager.Instance)
            {
                NetworkGameManager.Instance.OnAllPlayersJoined += BeginDisplaying;
                NetworkGameManager.Instance.OnGameEnd += EndDisplaying;
                
            }
            else
            {
                Debug.LogWarning("Dude what :/", gameObject);
                BeginDisplaying();
            }

        }

        private void OnDestroy()
        {
            if (NetworkGameManager.Instance)
            {
                NetworkGameManager.Instance.OnGameBegin -= BeginDisplaying;
                NetworkGameManager.Instance.OnGameEnd -= BeginDisplaying;
            }
        }


        public void BeginDisplaying()
        {
            Debug.Log("We are now displaying balls", gameObject);

            cam.enabled = true;
            podiumController.enabled = true;
        }

        public void EndDisplaying()
        {
            Debug.Log("We are not displaying balls anymore", gameObject);
            cam.enabled = false;
            podiumController.enabled = false;
        }
        
        
        public void TrySpawnSelectedBall(int i)
        {
            SaveManager.BallStructure myBall = SaveManager.MyBalls.GetReadonlyBall(i);
            Debug.Log("Selecting ball: " + myBall.ball, gameObject);
            if ((!NetworkGameManager.Instance.CanRespawn()) && NetworkManager.Singleton)
            {
                Debug.Log("Player cannot respawn right now!", gameObject);
                return;
            }

            podiumController.DisablePodiumAndCycle(i);
            Debug.LogWarning("Add server side check to see if we can still spawn that ball, or if we've already spent it.", gameObject);
            BallHandler.Instance.SpawnBall_ServerRpc(myBall.ball, myBall.weapon, myBall.ability);
            EndDisplaying();
            #if UNITY_EDITOR
            BallHandler.Instance.SpawnBall_Offline(myBall.ball, myBall.weapon, myBall.ability);
            #endif
            
        }
    }
}