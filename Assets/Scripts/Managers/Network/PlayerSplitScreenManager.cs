using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LocalMultiplayer;
using Managers.Local;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSplitScreenManager : NetworkBehaviour
{
    /// <summary>
    /// PLAYERSTATE ONLY GETS SPAWNED PER UNIQUE CLIENT.
    /// IF WERE SPLITSCREEN, THE "HOST" OF THE SPLITSCREEN WILL CREATE A PLAYERSTATE AND HANDLE MESSAGING TO ITS CHILDREN...
    /// </summary>

    public static PlayerSplitScreenManager Instance;
    
    
    private readonly List<PlayerController> _childrenPlayers = new List<PlayerController>();
    
    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        foreach (var player in SplitscreenPlayerManager.Instance.LocalPlayers)
        {
            PlayerController pc = player.GetComponent<PlayerController>();

            if (pc && !_childrenPlayers.Contains(pc))
            {
                _childrenPlayers.Add(pc);
            }
        }


        Debug.Log("FOUND CHILDREN IN SPLITSCREEN WITH A COUNT OF: " + _childrenPlayers.Count);
    }

    

    public PlayerController FindChild(int id)
    {
        for (int i = 0; i < _childrenPlayers.Count; i++)
        {
            if (_childrenPlayers[i].PlayerInput.playerIndex == id)
            {
                return _childrenPlayers[i];
            }
        }

        Debug.Log("COULDNT FIND CHILD PLAYER WITH ID: " + id);
        return null;
    }

    public PlayerController FindChild(FixedString64Bytes userName)
    {
        for (int i = 0; i < _childrenPlayers.Count; i++)
        {
            if (_childrenPlayers[i].gameObject.name.Contains(userName.Value))
            {
                return _childrenPlayers[i];
            }
        }

        Debug.Log("COULDNT FIND CHILD PLAYER WITH USERNAME: " + userName);
        return null;
    }
    
}
