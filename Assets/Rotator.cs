using System;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 speed;
    [SerializeField] private Vector3 startingRotation;

    private void Start()
    {
        transform.eulerAngles = startingRotation;
    }

#if UNITY_EDITOR
    [SerializeField] private bool runDuringEditor;
#endif
    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (!runDuringEditor) return;
        #endif
        
        transform.eulerAngles += speed * Time.deltaTime;
    }
}
