using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Pools
{
    public class ObjectPoolManager : MonoBehaviour
    {
        [Serializable]
        public struct ObjectPool
        {
            public int poolSize;
            public GameObject poolObject;
            public string poolName;
        }

    
        [SerializeField] private ObjectPool[] objectPools;


        public static ObjectPoolManager Instance { get; private set; }


        private Dictionary<string, Queue<GameObject>> Pool { get; set; } = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            InitPools();
        }

        private void InitPools()
        {
            foreach (ObjectPool pool in objectPools)
            {
                string poolName = pool.poolName;
                Pool[poolName] = new Queue<GameObject>();

                for (int i = 0; i < pool.poolSize; i++)
                {
                    GameObject poolObject = Instantiate(pool.poolObject, transform);

                    if (poolObject.TryGetComponent(out PooledObject pooledObject))
                    {
                        pooledObject.InitPoolObject(poolName);
                    }
                    else
                    {
                        PooledObject newPooledObject = poolObject.AddComponent<PooledObject>();
                        newPooledObject.InitPoolObject(poolName);
                    }
                
                    poolObject.SetActive(false);
                    Pool[poolName].Enqueue(poolObject);
                }
            }
        }


        public T GetObjectFromPool<T>(string poolName, Vector3 pos, Quaternion rot) where T : Component
        {
            if (Pool.ContainsKey(poolName) && Pool[poolName].Count > 0)
            {
                if (!Pool[poolName].TryDequeue(out GameObject obj))
                {
                    return null;
                }

                obj.transform.SetPositionAndRotation(pos, rot); 
                obj.SetActive(true);

                T component = obj.GetComponent<T>();

                if (component is PooledObject pooledObject)
                {
                    pooledObject.OnTakenFromPool();
                }
            
                if (component)
                {
                    return component;
                }
            
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogError($"Pooled object does not have a component of type {typeof(T)}.");
                return null;
            }
            Debug.LogError("The pool was empty, it must be too small... Do we implement dynamic pool spawning?");

            return null;
        }

        public void ReturnToPool(string poolName, GameObject poolObject)
        {
            if (!Pool.ContainsKey(poolName))
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogWarning($"Cannot return projectile of unknown type: {poolName}");
                return;
            }

            Debug.Log("Return?");
            poolObject.gameObject.SetActive(false);
            poolObject.transform.SetParent(transform);
            Pool[poolName].Enqueue(poolObject);
        }
    }
}
