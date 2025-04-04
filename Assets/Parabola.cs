using System;
using System.Collections.Generic;
using Stats;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(LineRenderer))]
public class Parabola : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject hitRadiusEffect;
    [SerializeField] private float maxFirePower = 20f;
    [SerializeField] private float chargeRate = 10f; 
    [SerializeField] private int resolution = 30;
    [SerializeField] private LayerMask groundMask;
    
    private LineRenderer _lr;
    [NonSerialized] public float FirePower;
    [NonSerialized] public Vector3 GravityModifier;

    [NonSerialized]public ProjectileStats PS;

    public void BindProjectile(ProjectileStats w)
    {
        PS = w;

    }

    public void ToggleLineRenderer(bool value)
    {
        enabled = value;
        _lr.enabled = value;
        hitRadiusEffect.SetActive(value);
    }

    private void Start()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.enabled = false;
        hitRadiusEffect.SetActive(false);
    }

    public void LateUpdate()
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 startPosition = firePoint.position;
        Vector3 startVelocity = firePoint.forward * (FirePower * PS.InitialVelocity);
        Vector3 currentPosition = startPosition;
        Vector3 currentVelocity = startVelocity;
        float timeStep = 0.05f;
        
        GravityModifier = PS.GravMult * Physics.gravity; // << These should be pre-computed in buiolds.

        for (int i = 0; i < resolution; i++)
        {
            points.Add(currentPosition);

            Vector3 nextPosition = currentPosition + currentVelocity * timeStep;
            currentVelocity += GravityModifier;

            if (Physics.Raycast(currentPosition, nextPosition - currentPosition, out RaycastHit hit, (nextPosition - currentPosition).magnitude, groundMask))
            {
                points.Add(hit.point);
                hitRadiusEffect.transform.position = hit.point;
                hitRadiusEffect.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                break;
            }

            currentPosition = nextPosition;
        }

        _lr.positionCount = points.Count;
        _lr.SetPositions(points.ToArray());
        
    }
}
