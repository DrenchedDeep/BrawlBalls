using UnityEngine;

public class Test : MonoBehaviour
{
    private ParticleSystem _ps;
    private Vector3 origin;
    
    // Start is called before the first frame update
    void Start()
    {
        _ps = GetComponent<ParticleSystem>();
        InvokeRepeating(nameof(Demo), 0, 0.4f);
        origin = transform.position;
    }

    private void Demo()
    {
        _ps.transform.position = origin + Random.insideUnitSphere * 4;
        _ps.Play();
    }
}
