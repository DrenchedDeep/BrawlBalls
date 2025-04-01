using System;
using Gameplay;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class BallPlayerHUD : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI nameTagText;
    [SerializeField] private float minDistToShowNameTag = 20;

    private BallPlayer _ballPlayer;
    
    private void LateUpdate()
    {
        if (_ballPlayer)
        {
            transform.position = _ballPlayer.transform.position;
        }
        
        if (Camera.main)
        {
            Vector3 dir = transform.position - Camera.main.transform.position;
            nameTagText.transform.rotation = Quaternion.LookRotation(nameTagText.transform.position - Camera.main.transform.position);

            nameTagText.gameObject.SetActive(dir.magnitude < minDistToShowNameTag );
        }
    }

    public void AttachTo(BallPlayer ballPlayer)
    {
        _ballPlayer = ballPlayer;
    }

    public void SetNameTag(string playerName)
    {
        nameTagText.text = playerName;
    }

    public void UpdateHealth(float current, float max)
    {
        float percentage = (current / max);
        
        healthSlider.maxValue = max;
        healthSlider.value = percentage;
    }
}
