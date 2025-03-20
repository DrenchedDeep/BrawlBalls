using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Weapons;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    [System.Serializable]
    public struct ProjectilePoolInfo
    {
        public string poolName;
        public int poolSize;
        public Projectile projectile;
    }

    public static ProjectileManager Instance;
    
    private Dictionary<string, Queue<Projectile>> ProjectilePool { get; set; } = new Dictionary<string, Queue<Projectile>>();

    [SerializeField] private ProjectilePoolInfo[] projectiles;

    //everybody should have their own pool of projectiles
    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializePool();
    }
    
    private void InitializePool()
    {
        foreach (ProjectilePoolInfo projectilePoolInfo in projectiles)
        {
            string poolName = projectilePoolInfo.poolName;
            ProjectilePool[poolName] = new Queue<Projectile>();

            for (int i = 0; i < projectilePoolInfo.poolSize; i++)
            {
                Projectile projectile = Instantiate(projectilePoolInfo.projectile, transform);
                projectile.gameObject.SetActive(false);
                ProjectilePool[poolName].Enqueue(projectile);
            }
        }
    }

    public Projectile GetProjectile(string type, Vector3 pos, Quaternion rot)
    {
        if (ProjectilePool.ContainsKey(type) && ProjectilePool[type].Count > 0)
        {
            Projectile obj = ProjectilePool[type].Dequeue();
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            obj.gameObject.SetActive(true);
            return obj.GetComponent<Projectile>();
        }

        return null;
    }


    public string GetProjectilePoolName(Projectile projectile)
    {
        foreach (var projectilePoolInfo in projectiles)
        {
            if (projectilePoolInfo.projectile == projectile)
            {
                return projectilePoolInfo.poolName;
            }
        }

        return "";
    }

    public void ReturnProjectile(string type, Projectile projectile)
    {
        if (!ProjectilePool.ContainsKey(type))
        {
            Debug.LogWarning($"Cannot return projectile of unknown type: {type}");
            return;
        }

        projectile.gameObject.SetActive(false);
        projectile.transform.SetParent(transform);
        ProjectilePool[type].Enqueue(projectile);
    }
}
