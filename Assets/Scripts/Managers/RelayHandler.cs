using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayHandler : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI lobbyCode;

    private float x = 1.1f;
    


    public static RelayHandler Instance { get; private set; }

    private void Start()
    {
        if(Instance) Destroy(gameObject);
        Instance = this;
    }


    public async Task CreateRelay(Lobby lobby)
    {
        // Decides the region
        try
        {
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
            
            //Get the join code so they can connect to the same relay
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            
            //Update the UI element to display and allow copying.
            lobbyCode.text = joinCode;

            RelayServerData relayServerData = new RelayServerData(alloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            
            
            /*
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData
                );
            */
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async Task JoinRelay(Lobby lb)
    {
        await JoinRelay(lb.LobbyCode);
    }


    public async Task JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining with code: " + joinCode);
            JoinAllocation alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(alloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            /*
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                alloc.RelayServer.IpV4,
                (ushort)alloc.RelayServer.Port,
                alloc.AllocationIdBytes,
                alloc.Key,
                alloc.ConnectionData,
                alloc.HostConnectionData
                );*/

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }
}
