using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageAble
{
    public void TakeDamage(float amount, Vector3 direction, ulong attacker);


}
