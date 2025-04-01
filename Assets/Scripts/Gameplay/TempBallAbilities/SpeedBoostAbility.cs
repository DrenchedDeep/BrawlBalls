using Cysharp.Threading.Tasks;
using UnityEngine;

public class SpeedBoostAbility : BallAbilityBase
{
    [SerializeField] private float accelerationMultiplier = 1.5f;
    [SerializeField] private float maxSpeedMultiplier = 1.5f;
    [SerializeField] private float resetTime = 5f;

    public bool IsSpeedBoosted { get; private set; }
    
    protected override void ActivateAbility()
    {
        Ball.ChangeAcceleration(accelerationMultiplier);
        Ball.ChangeMaxSpeed(maxSpeedMultiplier);

        _ = ResetSpeedBoost();
        IsSpeedBoosted = true;
    }

    private async UniTask ResetSpeedBoost()
    {
        await UniTask.WaitForSeconds(resetTime);
        
        Ball.ChangeAcceleration(accelerationMultiplier, true);
        Ball.ChangeMaxSpeed(maxSpeedMultiplier, true);
        IsSpeedBoosted = false;
    }

    
}
