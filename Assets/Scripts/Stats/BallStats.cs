using System.Text;
using UnityEngine;

namespace Stats
{
    [CreateAssetMenu(fileName = "Ball Stats", menuName = "Stats/BallStats", order = 1)]
    public class BallStats : BaseSelectorStats
    {
        [field: SerializeField] public float MaxHealth { get; private set; }
        [field: SerializeField] public float MaxSpeed { get; private set; }
        [field: SerializeField, Min(0.01f)] public float AngularDrag { get; private set; }
        [field: SerializeField, Min(0.01f)] public float Drag { get; private set; }
        [field: SerializeField] public float Acceleration { get; private set; }
        [field: SerializeField, Min(0.01f)] public float Mass { get; private set; }

        [field: SerializeField] public Material Material { get; private set; }
        [field: SerializeField] public Mesh Mesh { get; private set; }


        private string _commonTraits;

        protected override string CreateCommonTraits()
        {
            StringBuilder st = new();

            st.AppendLine($"<sprite=0>{MaxHealth}");
            st.AppendLine($"<sprite=4>{Mass}");
            st.AppendLine($"<sprite=1>{MaxSpeed}");
            st.AppendLine($"<sprite=2>{Drag}");
            st.AppendLine($"<sprite=3>{Acceleration}");
            
            return st.ToString();
        }
    }
}
