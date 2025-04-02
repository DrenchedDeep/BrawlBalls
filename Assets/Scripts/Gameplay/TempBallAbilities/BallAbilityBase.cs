using System;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Balls;
using Unity.Netcode;
using UnityEngine;

public class BallAbilityBase : NetworkBehaviour
{
    public enum BindTo
    {
        OnEnemyDefeated,
        OnDeath,
        OnDefeated,
        OnItemCollected,
        Manual
    }

    [SerializeField] private BindTo bindTo;

    protected Ball Ball;

    private void Awake()
    {
        Ball = GetComponent<Ball>();
    }
    

    public override void OnNetworkSpawn()
    {
        _ = Fuck();
    }

    private async UniTask Fuck()
    {
        await UniTask.WaitForSeconds(1);
        
        Ball = GetComponent<Ball>();

        if (IsOwner)
        {
            switch (bindTo)
            {
                case BindTo.OnDefeated:
                    Ball.BallPlayer.Owner.OnDefeated += ActivateWrapper;
                    break;
            
                case BindTo.OnEnemyDefeated:
                    Ball.BallPlayer.Owner.OnEnemyDefeated += ActivateWrapper;
                    break;
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            switch (bindTo)
            {
                case BindTo.OnDefeated:
                    Ball.BallPlayer.Owner.OnDefeated -= ActivateWrapper;
                    break;

                case BindTo.OnEnemyDefeated:
                    Ball.BallPlayer.Owner.OnEnemyDefeated -= ActivateWrapper;
                    break;
            }
        }
    }


    //scuffed :P 
    private void ActivateWrapper(int i, int x) => ActivateAbility();

    protected virtual void ActivateAbility()
    {
        
    }
    
    
}
