using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Parabola : MonoBehaviour
{
    [SerializeField, Min(0)] private int numNodes;
    [SerializeField, Range(0.1f, 10)] private float step;

    /*
    
    private Weapon _weapon;
    private LineRenderer _lr;
    private Vector3[] _positions;
    
    private static readonly float Grav = Physics.gravity.y/2;
    
    public void Init(Weapon w)
    {
        _weapon = w;
        _lr.enabled = true;
        transform.SetParent(w.firePoint);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        _weapon.OnCanShoot += () =>
        {
            _lr.enabled = true;
        };
        _weapon.OnShoot += () => _lr.enabled = false;
        _weapon.OnShootCancel += () => _lr.enabled = false;
    }


    // Start is called before the first frame update
    void Start()
    {
       
        _lr = GetComponent<LineRenderer>();
        _lr.enabled = false;
        _positions = new Vector3[numNodes];
        _lr.positionCount = numNodes;
        Init(transform.parent.GetComponent<Weapon>());
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!_lr.enabled) return;

        
        //The arrow has a forward force of weapon.currentFirePower();
        //The arrow has a downward force of gravity.
        //Then we need to set N nodes. X is sampling that value.
        float n = _weapon.CurrentFirePower();
        float velocity = _weapon.firePoint.right.y * n;
        for (int i = 1; i < numNodes; i++)
        {
            float time = i * step;
            float y = Grav * time * time + velocity * time; //ax^2 + bx + c (But C is automatically done because it's our position)
            Vector3 point = new Vector3(0, y, time * n);
            _positions[i] = point;
           // positions[i].Set(0,  n * i * i + grav * i, n*i);
        }
        
        
        _lr.SetPositions(_positions);
        
    }*/
}
