using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay;
using Unity.Netcode;
using UnityEngine;

public class SnowAuraAbility : BallAbilityBase
{
    public static float SlowAmount { get; set; } = 2;
    
    [SerializeField] private float slowTime = 5;
    [SerializeField] private float slowRadius = 20;


    private List<BallPlayer> _slowedPlayers = new List<BallPlayer>();
    

    protected override void ActivateAbility()
    {
        SlowPlayersInRadius_ServerRpc();
    }

    [ServerRpc]
    private void SlowPlayersInRadius_ServerRpc()
    {
        Collider[] colliders = new Collider[10];
        var size = Physics.OverlapSphereNonAlloc(transform.position, slowRadius, colliders);

        foreach (Collider col in colliders)
        {
            if (col.gameObject.TryGetComponent(out Gameplay.BallPlayer ballPlayer))
            {
                _slowedPlayers.Add(ballPlayer);
                ballPlayer.GetBall.IsSlowed.Value = true;
            }
        }

        _ = ResetSlowedPlayers();
    }


    private async UniTask ResetSlowedPlayers()
    {
        await UniTask.WaitForSeconds(slowTime);

        foreach (BallPlayer player in _slowedPlayers)
        {
            player.GetBall.IsSlowed.Value = false;
        }
    }
}
