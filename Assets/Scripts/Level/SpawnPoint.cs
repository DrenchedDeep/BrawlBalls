using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static SpawnPoint CurrentSpawnPoint { get; private set; }
    private Material mat;
    private static readonly int AmountID = Shader.PropertyToID("_amount");
    private const float ChangeTime = 0.6f;
    [SerializeField] private int id;
    public static readonly Dictionary<int ,SpawnPoint> ActiveSpawnPoints = new();
    
    private void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
        ActiveSpawnPoints.Add(id,this);
    }

    private void OnDestroy()
    {
        ActiveSpawnPoints.Remove(id);
    }


    //Can only collide with owner ball
    private void OnTriggerEnter(Collider other)
    {
        //If there is no spawn point
        if (!CurrentSpawnPoint)
        {
            SetActive();
            return;
        }

        if (CurrentSpawnPoint == this) return;
        
        CurrentSpawnPoint.StartCoroutine(CurrentSpawnPoint.ChangeColor());
        SetActive();
    }

    void SetActive()
    {
        StopAllCoroutines();
        StartCoroutine(ChangeColor());
        CurrentSpawnPoint = this;
    }

    private IEnumerator ChangeColor()
    {
        float cT = 0;
        float start = this == CurrentSpawnPoint?1:0;
        float end = 1-start;
        
        while (cT < ChangeTime)
        {
            cT += Time.deltaTime;
            mat.SetFloat(AmountID, Mathf.Lerp(start, end, cT/ChangeTime));
            yield return null;
        }
    }
    

}
