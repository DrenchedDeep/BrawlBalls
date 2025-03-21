using System;
using Managers.Local;
using UnityEngine;

namespace Gameplay.PlayerUtility
{
    [ExecuteInEditMode, RequireComponent(typeof(MeshRenderer))]
    public class GroundSphere : MonoBehaviour
    {
        [SerializeField] private GroundSphereInfo info;
        private MeshRenderer _mr;
        private void Awake()
        {
            _mr = GetComponent<MeshRenderer>();
        }

        void LateUpdate()
        {
            Transform tr = transform.parent;
            bool hitSomething = Physics.SphereCast(tr.position, info.MaxSize, Vector3.down, out RaycastHit hit, info.MaxDist,StaticUtilities.GroundLayers) && hit.distance > info.MinDist;
            _mr.enabled = hitSomething;
            if (hitSomething)
            {
                float size = info.GetSizeByDistance(hit.distance);
                transform.localScale = new Vector3(size, size, size );
                transform.position = hit.point;
            }
        }
    }
}
