using UnityEngine;

namespace Gameplay
{
    public interface IDamageAble
    {
        public void TakeDamage_ClientRpc(float amount, Vector3 direction, ulong attacker);
    }
}
