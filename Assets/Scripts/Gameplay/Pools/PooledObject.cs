using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public event Action OnTakenFromPoolEvent;
    public event Action OnReturnedToPoolEvent;


    protected string PoolName;
    
    public virtual void InitPoolObject(string poolName)
    {
        PoolName = poolName;
    }

    public virtual void OnTakenFromPool()
    {
        OnTakenFromPoolEvent?.Invoke();
    }
    
    public async UniTask ReturnToPoolTask(float time = 1f)
    {
        await UniTask.WaitForSeconds(time);
        ReturnToPool();
    }

    public virtual void ReturnToPool()
    {
        OnReturnedToPoolEvent?.Invoke();
        ObjectPoolManager.Instance.ReturnToPool(PoolName, gameObject);
    }
}