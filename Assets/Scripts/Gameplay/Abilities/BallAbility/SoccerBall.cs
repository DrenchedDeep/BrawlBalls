using System;
using Gameplay;
using Gameplay.Balls;
using Managers.Local;
using UnityEngine;


public class SoccerBall : Ball
{
    [SerializeField] private int maxInAirJumps = 1;
    
    private int _inAirJumpCount;

    protected override void Start()
    {
        base.Start();
        
        OnGroundStateChanged += GroundStateChanged;
    }

    private void GroundStateChanged()
    {
        if (!IsGrounded)
        {
            _inAirJumpCount = 0;
        }
    }
    
    
    protected override bool CanJump()
    {
        if (!IsGrounded)
        {
            return _inAirJumpCount < maxInAirJumps;
        }

        return true;
    }

    protected override void Jump()
    {
        base.Jump();

        if (!IsGrounded)
        {
            _inAirJumpCount++;
        }
    }
}
