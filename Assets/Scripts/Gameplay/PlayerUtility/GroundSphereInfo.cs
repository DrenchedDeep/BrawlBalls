using UnityEngine;

namespace Gameplay.PlayerUtility
{
    [CreateAssetMenu(fileName = "GroundSphereInfo", menuName = "PlayerUtility/GroundSphereInfo")]
    public class GroundSphereInfo : ScriptableObject
    {
        [SerializeField] private float minSize;
        [SerializeField] private float maxSize;
        [SerializeField] private float maxDistance;
        [SerializeField] private float minDistance = 1;
        [SerializeField] private AnimationCurve sizeCurve;

        public float MaxDist => maxDistance;
        public float MaxSize => maxSize;
        public float MinDist => minDistance;

        public float GetSizeByDistance(float distance) => Mathf.LerpUnclamped(minSize, maxSize, sizeCurve.Evaluate(Mathf.Clamp01(distance/maxDistance)));
    }
}
