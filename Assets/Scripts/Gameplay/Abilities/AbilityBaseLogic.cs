using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using Stats;
using UnityEngine;

public class AbilityBaseLogic : Ability
{
    private bool _isRecharging;
    private bool _hasEnoughAmmo;
    private CancellationTokenSource _rechargeCancelSource;
    
    public override bool CanUseAbility(BallPlayer owner)
    {
        return false;
    }


    public override void ExecuteAbility(BallPlayer owner) { }

    public override void CancelAbility(BallPlayer owner)
    {
        _rechargeCancelSource?.Cancel();
        _isRecharging = false;
    }
}
