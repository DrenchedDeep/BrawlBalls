using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ParticleManager : MonoBehaviour //Better called AbilityHelper
{
    

    
    [SerializeField] private VisualEffect[] effects;
    [SerializeField] private GameObject[] summonObjects;
    [SerializeField] private Material glueBallMat;
    [SerializeField] private Material protectMat;


    [Header("Portal")]
    [SerializeField, ColorUsage(false, true)] private Color[] primaryColors;
    [SerializeField, ColorUsage(false, true)] private Color[] secondaryColors;
    private static int _prv;
    public static Color GetRandomPrimaryColor => _pm.primaryColors[Random.Range(0, _pm.primaryColors.Length)];

    public static Color GetRandomSecondaryColor => _pm.secondaryColors[Random.Range(0, _pm.secondaryColors.Length)];

    private static ParticleManager _pm;


    [SerializeField] private AnimationCurve ExplosiveDropoff;

    public static float EvalauteExplosiveDistance(float percent) => _pm.ExplosiveDropoff.Evaluate(percent);

    public static Material GlueBallMat => _pm.glueBallMat;
    public static Material ProtectMat => _pm.protectMat;
    public static readonly Dictionary<string, VisualEffect> VFX = new();
    public static readonly Dictionary<string, GameObject> SummonObjects = new();

    public enum ECollectableType
    {
        Gems,
        //TOKEN TYPES
        Cosmetic,
        Ability,
        Weapon,
        Ball,
        Special
    }


    private void Awake()
    {
        if(_pm)
            Destroy(gameObject);
        _pm = this;

        //Compile in main menu... Can be slow if massive...
        //Pooling...
        foreach (VisualEffect effect in effects)
        {
            VFX.Add(effect.name, Instantiate(effect,transform)); // Array Pool local...
        }
        
        foreach (GameObject effect in summonObjects)
        {
            SummonObjects.Add(effect.name, effect);
            print(effect.name);
        }


        DontDestroyOnLoad(gameObject);
    }

    public static void InvokeParticle(string id, Vector3 position)
    {
        print("Invoking particle: " + id);
        
        VFX[id].SetVector3(StaticUtilities.PositionID, position);
        VFX[id].SendEvent(StaticUtilities.ActivateID);
    }

    
}
