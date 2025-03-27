using Managers.Network;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Jump : Ability
    {
        private bool _setupDelegates;
        private int _inAirJumpCount;

        private const int MAX_IN_AIRJUMP_COUNT = 1;

        public override bool CanUseAbility(BallPlayer owner)
        {
            return !owner.GetBall.IsGrounded && _inAirJumpCount < MAX_IN_AIRJUMP_COUNT;
        }

        public override void ExecuteAbility(BallPlayer owner)
        {
            //perhaps having an INIT ability function would make sense...
            if (!_setupDelegates)
            {
                owner.GetBall.OnGroundStateChanged += () => OnGroundStateChanged(owner);
                _setupDelegates = true;
            }


            _inAirJumpCount++;
            owner.GetBall.Jump(false);
        }
        
        private void OnGroundStateChanged(BallPlayer owner)
        {
            if (owner.GetBall.IsGrounded)
            {
                _inAirJumpCount = 0;
            }
        }

        public override void CancelAbility(BallPlayer owner)
        {
     //       _inAirJumpCount = 0;
        }
    }
}
