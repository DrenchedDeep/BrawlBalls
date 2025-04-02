using System;
using System.Globalization;
using Managers.Local;
using TMPro;
using UnityEngine;
using MathF = System.MathF;

public class SpeedMeter : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform needle;
    [SerializeField] private TextMeshProUGUI speedText;
    
    [SerializeField] private float minAngle;
    [SerializeField] private float maxAngle;

    private float _currentSpeed;

    private void Update()
    {
        if (!playerController.CurrentBall)
        {
            return;
        }

        _currentSpeed = playerController.CurrentBall.GetBall.Speed;
        
        float maxSpeed = playerController.CurrentBall.GetBall.MaxSpeed;

        float speedPercent = Mathf.Clamp(_currentSpeed / maxSpeed, 0, 1);
        Debug.Log(speedPercent);
        speedText.text = ((int)_currentSpeed).ToString(CultureInfo.CurrentCulture);
        float angle = Mathf.Lerp(minAngle, maxAngle, speedPercent);
        float shake = Mathf.Sin(Time.time * 50f) * speedPercent;
        
        needle.localRotation = Quaternion.Euler(0, 0, angle + shake);
    }
}
