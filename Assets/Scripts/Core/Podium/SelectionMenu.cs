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
        

        private void Awake()
        {
            transform.root.GetComponent<PlayerController>().SetSelectionMenu(this);
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
            if (!SaveManager.TryGetPlayerData(podiumController.localPlayerInputComponent, out SaveManager.PlayerData playerData))
            {
                Debug.LogError("This save data does not exist: " + playerData.Username, gameObject);
                return;
            }
            SaveManager.BallStructure myBall = playerData.GetReadonlyBall(i);
            Debug.Log("Selecting ball: " + myBall.ball, gameObject);
            if ((!NetworkGameManager.Instance.CanRespawn()) && NetworkManager.Singleton)
            {
                Debug.Log("Player cannot respawn right now!", gameObject);
                return;
            }

            podiumController.DisablePodiumAndCycle(i);
            Debug.LogWarning("Add server side check to see if we can still spawn that ball, or if we've already spent it.", gameObject);
#if UNITY_EDITOR
            if(!NetworkManager.Singleton || !NetworkManager.Singleton.IsConnectedClient)
                BallHandler.Instance.SpawnBall_Offline(myBall.ball, myBall.weapon, myBall.ability, playerData.PlayerIndex);
            else
            {
                BallHandler.Instance.SpawnBall_ServerRpc(myBall.ball, myBall.weapon, myBall.ability, playerData.PlayerIndex);
                Debug.Log("is online spawn ball request");
            }
#else
            BallHandler.Instance.SpawnBall_ServerRpc(myBall.ball, myBall.weapon, myBall.ability, playerData.PlayerIndex);
#endif
            EndDisplaying();
        }
    }
}