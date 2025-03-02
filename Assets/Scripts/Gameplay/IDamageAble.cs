using UnityEngine;

namespace Gameplay
{
    public interface IDamageAble
    {
        public void TakeDamageClientRpc(float amount, Vector3 direction, ulong attacker);
    }
}
