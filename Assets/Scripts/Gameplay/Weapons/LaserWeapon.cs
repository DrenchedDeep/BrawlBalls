using System;
using Managers.Local;
using RotaryHeart.Lib.PhysicsExtension;
using Unity.Netcode;
using UnityEngine;
using Physics = UnityEngine.Physics;

public class LaserWeapon : ProjectileWeaponBase
{
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private float sphereCastRange;

    public readonly RaycastHit[] Hits = new RaycastHit[10];

    private void Update()
    {
        Transform tr = transform;
        Vector3 position = tr.position;
        Vector3 forward = tr.forward;

        
        bool hitWall = Physics.Raycast(position, forward, out RaycastHit wallCheck, stats.MaxRange, StaticUtilities.GroundLayers);
        float dist = hitWall?wallCheck.distance:stats.MaxRange;

        int hitCount = Physics.SphereCastNonAlloc(position, stats.MaxRadius, forward, Hits, dist, StaticUtilities.EnemyLayer);
    }
    
    
}
