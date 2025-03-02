using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayHandler : MonoBehaviour
{
    //private float x = 1.1f;
    
    public static RelayHandler Instance { get; private set; }

    private void Start()
    {
        if(Instance) Destroy(gameObject);
        Instance = this;
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            
            print("I connected as: " + id);
            if (!NetworkManager.Singleton.IsHost) return;
            foreach (var variable in  NetworkManager.Singleton.ConnectedClientsIds)
            {
                print("Clients connected: " + variable);                
            }
            
            LobbyHandler.ConnectedToRelay();
        };

    }


    public async Task<string> CreateRelay(int players)
    {
        // Decides the region
        try
        {
            LoadingHelper.Activate();
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(players);
            
            //Get the join code so they can connect to the same relay
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            
            Debug.Log("Creating Relay: " + joinCode);

            RelayServerData relayServerData = new RelayServerData(alloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            print("Connected to Relay: " + NetworkManager.Singleton.IsHost);
            
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
        return null;
    }
    

    public async Task JoinRelay(string joinCode)
    {
        try
        {
            LoadingHelper.Activate();
            Debug.Log("Joining with code: " + joinCode);
            JoinAllocation alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(alloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            print("Connected to Relay: " + NetworkManager.Singleton.IsClient);

            
            
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }
}
