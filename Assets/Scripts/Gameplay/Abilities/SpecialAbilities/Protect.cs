using Cysharp.Threading.Tasks;
using Managers;
using Managers.Local;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Protect : Ability
    {
        public override bool CanUseAbility(BallPlayer owner) => true;

        public override void ExecuteAbility(BallPlayer owner)
        {
            Debug.LogWarning("We're not tracking the immortality timer. Will this cause issues?");
            _ = ImmortalityTimer(owner);
        }

        public override void CancelAbility(BallPlayer owner)
        {
        }

        private async UniTask ImmortalityTimer(BallPlayer owner)
        {
            Debug.Log("Immortality_Start");
        
            //  owner.GetBall.ApplyEffect_ServerRpc(1);

            if (owner.GetBall)
            {
                owner.GetBall.IsProtected.Value = true;
            }

            await UniTask.WaitForSeconds(10);

            if (owner.GetBall)
            {
                Debug.Log("Immortaltiy_End");
                owner.GetBall.IsProtected.Value = false;
            }
            
           // owner.GetBall.RemoveEffect_ServerRpc(1, ParticleManager.ProtectMat.GetHashCode() );
        }

    }
}
