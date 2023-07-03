using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class RelayHandler : MonoBehaviour
{
    [SerializeField] private Button onlineSectionBtn;
    [SerializeField] private TextMeshProUGUI lobbyCode;
    
    // Start is called before the first frame update
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);
            
            //If successful, allow access to online services section...
            onlineSectionBtn.enabled = true;
        };

        //Access Authentication sercices
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

    }
    
    public async void CreateRelay()
    {
        // Decides the region
        try
        {
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(8);
            
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

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining with code: "+ joinCode);
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
