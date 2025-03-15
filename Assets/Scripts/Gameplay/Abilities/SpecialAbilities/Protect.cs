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

        private async UniTask ImmortalityTimer(BallPlayer owner)
        {
            Debug.Log("Immortality_Start");
        
            int refMat = ParticleManager.ProtectMat.GetHashCode();
            owner.GetBall.ApplyEffect_ServerRpc(1);

            await UniTask.Delay(3000);
            
            Debug.Log("Immortaltiy_End");
        
            owner.GetBall.RemoveEffect_ServerRpc(refMat);
        }

    }
}
