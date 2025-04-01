using Unity.Netcode;
using UnityEngine;

public class MassAbility : BallAbilityBase
{
    [SerializeField] private float massGainAmt = 1.1f;
    
    protected override void ActivateAbility()
    {
        Debug.Log("Activiate Ability thhangs");
        GainMass_ServerRpc();
    }

    [ServerRpc]
    private void GainMass_ServerRpc()
    {
        Ball.BallPlayer.transform.localScale *= massGainAmt;
        Ball.BallPlayer.RestoreHealth(); 
        Ball.BallPlayer.IncreaseMaxHealth(massGainAmt);
        Ball.BallPlayer.Rb.mass *= massGainAmt;
    }
}
