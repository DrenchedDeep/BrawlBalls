using UnityEngine;

namespace Gameplay
{
    public interface IDamageAble
    {
        public void TakeDamage_ServerRpc(DamageProperties damageInfo);
    }
}
