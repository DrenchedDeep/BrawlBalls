using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.VFX;

public class ParticleManager : MonoBehaviour
{
    private static readonly int PositionID = Shader.PropertyToID("Position");
    private static readonly int ActivateID = Shader.PropertyToID("Activate");
    [SerializeField] private VisualEffect[] effects;
    public static readonly Dictionary<string, VisualEffect> VFX = new();
    private static bool _created;
    private void Awake()
    {
        if(_created)
            Destroy(gameObject);
        _created = true;
        foreach (VisualEffect effect in effects)
        {
            print(effect.name);
            VFX.Add(effect.name, Instantiate(effect,transform));
        }
       
        DontDestroyOnLoad(gameObject);
    }

    public static void InvokeParticle(string id, Vector3 position)
    {
        print("Invoking particle: " + id);
        
        VFX[id].SetVector3(PositionID, position);
        VFX[id].SendEvent(ActivateID);
    }
}
