using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;

public class LobbyHandler : MonoBehaviour
{
    private Lobby myLobby;

    private void Start()
    {
       // QuickPlay();
    }

    public void LeaveLobby()
    {
    }
/*


    async void QuickPlay()
    {
        try
        {
            // Quick-join a random lobby with a maximum capacity of 10 or more players.
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            options.Filter = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.MaxPlayers,
                    op: QueryFilter.OpOptions.GE,
                    value: "10")
            };
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            // ...
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);

            if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
            {
                CreateLobby
            }
        }
    }
    
    Dictionary<string, PlayerDataObject> CreateInitialPlayerData(LocalPlayer user)
    {
        Dictionary<string, PlayerDataObject> data = new Dictionary<string, PlayerDataObject>();

        var displayNameObject =
            new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, user.DisplayName.Value);
        data.Add("DisplayName", displayNameObject);
        return data;
    }
    
    public async Task<Lobby> QuickJoinLobbyAsync(LocalPlayer localUser, LobbyColor limitToColor = LobbyColor.None)
    {
        //We dont want to queue a quickjoin
        if (m_QuickJoinCooldown.IsCoolingDown)
        {
            UnityEngine.Debug.LogWarning("Quick Join Lobby hit the rate limit.");
            return null;
        }

        await m_QuickJoinCooldown.QueueUntilCooldown();
        var filters = LobbyColorToFilters(limitToColor);
        string uasId = AuthenticationService.Instance.PlayerId;

        var joinRequest = new QuickJoinLobbyOptions
        {
            Filter = filters,
            Player = new Player(id: uasId, data: CreateInitialPlayerData(localUser))
        };

        return m_CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(joinRequest);
    }*/
    
}
