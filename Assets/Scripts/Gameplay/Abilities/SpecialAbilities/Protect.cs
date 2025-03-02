using Cysharp.Threading.Tasks;
using Gameplay.Balls;
using Managers;
using UnityEngine;

namespace Gameplay.Abilities.SpecialAbilities
{
    public class Protect : Ability
    {
    //
        public override bool CanUseAbility(NetworkBall owner, Weapon weapon)
        {
            return true;
        }

        protected override void UseAbility(NetworkBall owner, Weapon weapon)
        {
            Debug.LogWarning("We're not tracking the immortality timer. Will this cause issues?");
            _ = ImmortalityTimer(owner);
        }

        private async UniTask ImmortalityTimer(NetworkBall owner)
        {
            Debug.Log("Immortality_Start");
        
            int refMat = ParticleManager.ProtectMat.GetHashCode();
            owner.ApplyEffectServerRpc(1);

            await UniTask.Delay(3000);
            
            Debug.Log("Immortaltiy_End");
        
            owner.RemoveEffectServerRpc(refMat);
        }

    }
}
