using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageAble
{
    public void TakeDamageClientRpc(float amount, Vector3 direction, ulong attacker);


}
