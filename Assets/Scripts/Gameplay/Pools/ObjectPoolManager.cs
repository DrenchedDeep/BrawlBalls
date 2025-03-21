using System;
using System.Collections.Generic;
using Gameplay.Weapons;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public struct ObjectPool
    {
        public int poolSize;
        public GameObject poolObject;
        public string poolName;
    }

    
    [SerializeField] private ObjectPool[] objectPools;


    public static ObjectPoolManager Instance;
    
    
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
        Type type = typeof(T);
        if (Pool.ContainsKey(poolName) && Pool[poolName].Count > 0)
        {
            GameObject obj = Pool[poolName].Dequeue();
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            obj.gameObject.SetActive(true);

            T component = obj.GetComponent<T>();

            if (obj.TryGetComponent(out PooledObject pooledObject))
            {
                pooledObject.OnTakenFromPool();
            }
            
            if (component)
            {
                return obj.GetComponent<T>();
            }
            
            Debug.LogError($"Pooled object does not have a component of type {typeof(T)}.");
            return null;
        }

        return null;
    }

    public void ReturnToPool(string poolName, GameObject poolObject)
    {
        if (!Pool.ContainsKey(poolName))
        {
            Debug.LogWarning($"Cannot return projectile of unknown type: {poolName}");
            return;
        }
        
        poolObject.gameObject.SetActive(false);
        poolObject.transform.SetParent(transform);
        Pool[poolName].Enqueue(poolObject);
    }
}
