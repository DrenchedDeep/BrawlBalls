using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallPlayerHUD : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider damageSlider;

    [SerializeField] private TextMeshProUGUI nameTagText;
    [SerializeField] private Transform rotationPivot;
    [SerializeField] private float minDistToShowNameTag = 20;

    private BallPlayer _ballPlayer;
    private Camera _mainCamera;
    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_ballPlayer)
        {
            transform.position = _ballPlayer.transform.position;
        }
        
        if (_mainCamera)
        {
            Vector3 dir = transform.position - _mainCamera.transform.position;
            rotationPivot.transform.rotation = Quaternion.LookRotation(nameTagText.transform.position - _mainCamera.transform.position);
            rotationPivot.gameObject.SetActive(dir.magnitude < minDistToShowNameTag );
        }

        if (!Mathf.Approximately(damageSlider.value, healthSlider.value))
        {
            damageSlider.value = Mathf.Lerp(damageSlider.value, healthSlider.value, Time.deltaTime * 3);
        }
    }

    public void AttachTo(BallPlayer ballPlayer)
    {
        _ballPlayer = ballPlayer;
        _ballPlayer.OnDamaged += UpdateHealth;
        _ballPlayer.OnDestroyed += (arg1, i) => Destroy(gameObject);
    }
    
    public void SetNameTag(string playerName)
    {
        nameTagText.text = playerName;
    }

    public void UpdateHealth(float current, float max)
    {
        float percentage = (current / max);
        
       // healthSlider.maxValue = max;
        healthSlider.value = percentage;
    }
}
