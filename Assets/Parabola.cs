using System;
using System.Collections.Generic;
using UnityEngine;

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
    private float _firePower;



    public void UpdateFirePower(float firePower)
    {
        _firePower = firePower;
    }

    public void ToggleLineRenderer(bool value)
    {
        _lr.enabled = value;
        hitRadiusEffect.SetActive(value);
    }

    private void Start()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.enabled = false;
        hitRadiusEffect.SetActive(false);
    }

    private void Update()
    {
        if (_lr.enabled)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3 startPosition = firePoint.position;
            Vector3 startVelocity = firePoint.forward * _firePower;
            Vector3 currentPosition = startPosition;
            Vector3 currentVelocity = startVelocity;
            float timeStep = 0.05f;

            for (int i = 0; i < resolution; i++)
            {
                points.Add(currentPosition);

                Vector3 nextPosition = currentPosition + currentVelocity * timeStep;
                currentVelocity += Physics.gravity * timeStep;

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
    /*/
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0)] private int numNodes;
    [SerializeField, Range(0.1f, 10)] private float step;
    
    
    private LineRenderer _lr;
    private Vector3[] _positions;
    
    private static readonly float Grav = Physics.gravity.y/2;
    
    private float _firePower;

    private void Awake()
    {
    }

    public void ToggleLineRenderer(bool value) => _lr.enabled = value;


    // Start is called before the first frame update
    void Start()
    {
       
        _lr = GetComponent<LineRenderer>();
        _lr.enabled = false;
        _positions = new Vector3[numNodes];
        _lr.positionCount = numNodes;
        //  Init(transform.parent.GetComponent<Weapon>());
      
        transform.SetParent(firePoint);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

    }

    public void ClearLineRenderer()
    {
    }

    public void UpdateFirePower(float firePower) => _firePower = firePower;

    void LateUpdate()
    {
        if (!_lr.enabled) return;

        
        //The arrow has a forward force of weapon.currentFirePower();
        //The arrow has a downward force of gravity.
        //Then we need to set N nodes. X is sampling that value.
        float n = _firePower;
        float velocity = firePoint.forward.y * n;
        for (int i = 1; i < numNodes; i++)
        {
            float time = i * step;
            float y = Grav * time * time + velocity * time; //ax^2 + bx + c (But C is automatically done because it's our position)
            Vector3 point = transform.position + new Vector3(0, y, time * n);
            _positions[i] = point;
           // positions[i].Set(0,  n * i * i + grav * i, n*i);
        }
        
        
        _lr.SetPositions(_positions);
        
    }
    /*/
}
