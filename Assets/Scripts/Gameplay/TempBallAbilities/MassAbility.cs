using Unity.Netcode;
using UnityEngine;

public class MassAbility : BallAbilityBase
{
    [SerializeField] private float massGainAmt = 1.1f;
    
    protected override void ActivateAbility()
    {
        Ball.BallPlayer.transform.localScale *= massGainAmt;
        Ball.BallPlayer.Rb.mass *= massGainAmt;
        GainMass_ServerRpc();
    }

    [ServerRpc]
    private void GainMass_ServerRpc()
    {
        Debug.Log("DID THE THING");

     //   Ball.BallPlayer.transform.localScale *= massGainAmt;
     //   Ball.BallPlayer.Rb.mass *= massGainAmt;
        Ball.BallPlayer.RestoreHealth(); 
        Ball.BallPlayer.IncreaseMaxHealth(massGainAmt);
    }
}
