using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Gameplay.Map.Clockwork
{
    [RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
    public class RotatingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationAxis = new Vector3(0,1,0);
        [SerializeField, Tooltip("How far a \'tick\' really is")] private float tickDegrees = 2;
        [SerializeField, Tooltip("Time between rotations, can be used to make it step")] private float timeBetweenRotations; // Use this to make it look like it's "Ticking"

        private float _currentTime;

        private void Awake()
        {
            rotationAxis.Normalize();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            _currentTime += Time.deltaTime;

            if (_currentTime < timeBetweenRotations) return;
            _currentTime = 0;
            transform.localRotation *= Quaternion.AngleAxis(tickDegrees, rotationAxis);
        }
    }
}
