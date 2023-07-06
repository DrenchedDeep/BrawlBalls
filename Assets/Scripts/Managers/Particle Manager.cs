using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;
using UnityEngine.VFX;

public class ParticleManager : MonoBehaviour //Better called AbilityHelper
{
    
    public static readonly int RandomTexID = Shader.PropertyToID("_Tex");
    public static readonly int ColorID = Shader.PropertyToID("_Color");
    public static readonly int RandomOffsetID = Shader.PropertyToID("_Offset");
    
    //Particle
    public static readonly int PositionID = Shader.PropertyToID("Position");
    public static readonly int ActivateID = Shader.PropertyToID("Activate");
    public static readonly int EndID = Shader.PropertyToID("End");
    public static readonly int DelayID = Shader.PropertyToID("initDelay");
    
    [SerializeField] private VisualEffect[] effects;
    [SerializeField] private GameObject[] summonObjects;
    [SerializeField] private Material glueBallMat;
    public static Material GlueBallMat { get; private set; }
    public static readonly Dictionary<string, VisualEffect> VFX = new();
    public static readonly Dictionary<string, GameObject> SummonObjects = new();
    private static bool _created;


    private void Awake()
    {
        if(_created)
            Destroy(gameObject);

        GlueBallMat = glueBallMat;
        
        _created = true;
        //Compile in main menu... Can be slow if massive...
        foreach (VisualEffect effect in effects)
        {
            VFX.Add(effect.name, Instantiate(effect,transform)); // Array Pool local...
        }
        
        foreach (GameObject effect in summonObjects)
        {
            SummonObjects.Add(effect.name, effect);
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
