using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class ParticleManager : MonoBehaviour //Better called AbilityHelper
    {
    
        [SerializeField] private GameObject[] summonObjects;
        [SerializeField] private Material glueBallMat;
        [SerializeField] private Material protectMat;


        [Header("Portal")]
        [SerializeField, ColorUsage(false, true)] private Color[] primaryColors;
        [SerializeField, ColorUsage(false, true)] private Color[] secondaryColors;
        public static Color GetRandomPrimaryColor => Pm.primaryColors[Random.Range(0, Pm.primaryColors.Length)];

        public static Color GetRandomSecondaryColor => Pm.secondaryColors[Random.Range(0, Pm.secondaryColors.Length)];

        private static ParticleManager Pm { get; set; }




        [SerializeField] private AnimationCurve ExplosiveDropoff;

        public static float EvalauteExplosiveDistance(float percent) => Pm.ExplosiveDropoff.Evaluate(percent);

        public static Material GlueBallMat => Pm.glueBallMat;
        public static Material ProtectMat => Pm.protectMat;
        private static readonly Dictionary<string, ParticleSystem> Particles = new();
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
            if (Pm)
            {
                Destroy(gameObject);
                return;
            }

            Pm = this;

            Transform parent = transform.GetChild(1);
            int n = parent.childCount;
            for(int i =0; i < n; ++i)
            {
                Transform t = parent.GetChild(i);
                print("Registed particle: " + t.name);
                Particles.Add(t.name, t.GetComponent<ParticleSystem>());
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
            print("Invoking particle: " + id + " at position: " + position);
            Particles[id].transform.position = position;
            Particles[id].Play();
        }
    }

    public struct MaterialInfo<T>
    {
        public enum EMessageType
        {
            Int,
            Float,
            Vec4,
            Vec3
        }

        public EMessageType MessageType;
        public int hash;
        public T value;

    }
}