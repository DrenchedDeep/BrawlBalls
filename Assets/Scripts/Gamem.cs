using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using MainMenu.UI;
using Managers.Local;
using Unity.Netcode;
using UnityEngine;
public class Gamem : NetworkBehaviour
{
    [SerializeField, Min(0)] private float matchTime = 300;
    [SerializeField, Min(0)] private float timeToMatchStart = 15;
    [SerializeField,  Min(0)] private float matchOverTimeDuration = 120;
    
    public static Gamem Instance { get; private set; }
    
    private readonly HashSet<ulong> _players = new();


    public event Action OnHostDisconnected;
    public event Action OnAllPlayersJoined;
    
    public event Action OnGameBegin;
    public event Action OnGameEnd;
    

    private CancellationTokenSource _matchCancelTokenSource;

    private readonly SortedList<float, Action> _timedMatchEvents = new();

    public NetworkVariable<bool> GameStarted { get; private set; } = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> TotalTimePassed { get; private set; } = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> CurrentTime { get; private set; } = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> CurrentTimePeriod { get; private set; } = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        //every client should make their own instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GameStarted.OnValueChanged += OnGameStarted_Multicast;
        CurrentTime.OnValueChanged += OnCurrentTime_Multicast;
    }
    
    private void OnEnable()
    {
        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.OnServerStopped += OnServerStopped;
    }
    private void OnDisable()
    {
        NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.OnServerStopped -= OnServerStopped;
    }
    
    void OnClientConnected(ulong clientId)
    {
        _players.Add(clientId);
        
        CheckStartGame();
    }
    
    void OnClientDisconnected(ulong clientId)
    {
        _players.Remove(clientId);
    }
        
    private void OnServerStopped(bool obj)
    {
        Debug.LogError("Server stopped IMPLEMENT RETURN TO MENU OR HOST MIGRATION.");
        OnHostDisconnected?.Invoke();
    }

    private void CheckStartGame()
    {
        if (_players.Count == NetworkManager.ConnectedClients.Count)
        {
            GameStarted.Value = true;
            OnAllPlayersJoined?.Invoke();
            
            //only the server should be updating the match timers... clients can get that info through the NetworkedVariables, this is to keep it consistent 
            _matchCancelTokenSource =  new CancellationTokenSource();
            _ = ManagerMatchTimer(_matchCancelTokenSource.Token);
        }
    }

    //called when the gamestarted variable has replicated to the client....
    private void StartGame_Client()
    {
        Debug.LogWarning("Client knows the game is starting...");
        LoadingHelper.Deactivate();

    }

    private void OnGameStarted_Multicast(bool old, bool current)
    {
        if (current)
        {
            StartGame_Client();
            OnGameBegin?.Invoke();
        }
        else
        {
            OnGameEnd?.Invoke();
        }
    }

    private void OnCurrentTime_Multicast(float old, float current)
    {
        
    }
    
    private async UniTask ManagerMatchTimer(CancellationToken token)
    {
        await ProcessTimeFrame(timeToMatchStart, token);
            
        if (token.IsCancellationRequested)
        {
            Debug.LogWarning("MATCH IS BEING CANCELLED DURING INIT");
            return;
        }
            
        Debug.Log("Game is now beginning fully! Let the games begin!");
        OnGameBegin?.Invoke();
            
        await ProcessTimeFrame(matchTime, token);
            
        if (token.IsCancellationRequested)
        {
            Debug.LogWarning("MATCH IS BEING CANCELLED DURING GAMEPLAY");
            return;
        }
            
        Debug.Log("Now Entering overtime!");
            
        await ProcessTimeFrame(matchOverTimeDuration, token);
            
        if (token.IsCancellationRequested)
        {
            Debug.LogWarning("MATCH IS BEING CANCELLED DURING END GAME");
            return;
        }
            
        Debug.LogWarning("Well I guess everyone who's alive is a winner! TODO: Actually make this work");
    }
    
    private async UniTask ProcessTimeFrame(float duration, CancellationToken token)
    {
        CurrentTimePeriod.Value = duration;
        while (CurrentTime.Value < CurrentTimePeriod.Value)
        {
            float dt = Time.deltaTime;
            CurrentTime.Value += dt;
            TotalTimePassed.Value += dt;

            if (_timedMatchEvents.Count != 0 && GetTotalTimePassed >= _timedMatchEvents.Keys[0])
            {
                Debug.Log("Executing a timed event at time: " + GetTotalTimePassed);
                _timedMatchEvents.Remove(_timedMatchEvents.Keys[0]);
                _timedMatchEvents.Values[0].Invoke();
            }
            await UniTask.Yield(token);
        }
        CurrentTime.Value = 0;
    }

    public float EvaluateCurrentTimePeriodAsPercent => CurrentTime.Value / CurrentTimePeriod.Value;
    public float GetRemainingSectionTime => CurrentTimePeriod.Value - CurrentTime.Value;
    public float GetRemainingTime => GetTotalMatchTime - GetTotalTimePassed;
    public float GetTotalMatchTime => timeToMatchStart + matchTime + matchOverTimeDuration;
    public float GetTotalTimePassed => TotalTimePassed.Value;
    
    public void AddTimedEvent(float time, Action executedFunction)
    {
        _timedMatchEvents.Add(time, executedFunction);
    }

    public bool CanRespawn()
    {
        return Mathf.Approximately(CurrentTimePeriod.Value, matchTime);
    }
    
    
    //ideally particles shouldn't be spawned with RPC'S, they should be spawned with replicated variables... atleast in unreal, not sure in this.. so leaving it as is for now.
    [ServerRpc(RequireOwnership = false)]
    public void PlayParticleGlobally_ServerRpc(string particleName, Vector3 location, Quaternion rotation) => PlayParticleGlobally_ClientRpc(particleName, location,rotation);
    
                
    [ClientRpc]
    private void PlayParticleGlobally_ClientRpc(string particleName, Vector3 location, Quaternion rotation) => ParticleManager.InvokeParticle(particleName, location, rotation);

        
    [ServerRpc(RequireOwnership = false)]
    public void SpawnObjectGlobally_ServerRpc(string objectName, Vector3 location, Quaternion rotation, ServerRpcParams @params = default)
    {
        NetworkObject ngo = Instantiate(ResourceManager.SummonableObjects[objectName], location, rotation);
        ngo.SpawnWithOwnership(@params.Receive.SenderClientId);
    }

    [ClientRpc]
    public void SendMessage_ClientRpc(string s, float d, ClientRpcParams x = default) => _ = MessageManager.Instance.HandleScreenMessage(s, d);
}
