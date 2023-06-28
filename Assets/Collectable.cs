using System;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    private static readonly int PosID = Shader.PropertyToID("_Pivot");
    private Material m;
    void Awake()
    {
        m = GetComponent<MeshRenderer>().material;
    }
    private void Start()
    {
        m.SetVector(PosID, transform.position);
    }
    
}
