using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Pools
{
    public class PooledObject : MonoBehaviour
    {
        
        
        public event Action OnTakenFromPoolEvent;
        public event Action OnReturnedToPoolEvent;


        protected string PoolName;
        
        protected CancellationTokenSource PoolCancellation;
    
        public virtual void InitPoolObject(string poolName)
        {
            PoolName = poolName;
        }

        public virtual void OnTakenFromPool()
        {
            OnTakenFromPoolEvent?.Invoke();
        }
    
        public async UniTask ReturnToPoolTask(CancellationTokenSource cancellationTokenSource, float time = 1f)
        {
            await UniTask.WaitForSeconds(time);
            
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }
            
            ReturnToPool();
        }

        public virtual void ReturnToPool()
        {
        //    OnReturnedToPoolEvent?.Invoke();
            ObjectPoolManager.Instance.ReturnToPool(PoolName, gameObject);
        }
    }
}