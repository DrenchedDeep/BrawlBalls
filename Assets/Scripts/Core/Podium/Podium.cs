using Cysharp.Threading.Tasks;
using Gameplay;
using Managers.Local;
using Unity.Netcode;
using UnityEngine;

namespace Core.Podium
{
    public class Podium : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Transform ballPoint;
     
        private Material _material;
        private BallPlayer _myBall;
        
        [Header("Color Transition")]
        [SerializeField, ColorUsage(true, true)] private Color inactiveColor;
        [SerializeField, ColorUsage(true, true)] private Color activeColor;
        [SerializeField, ColorUsage(true, true)] private Color blockedColor;
        [SerializeField] private AnimationCurve colorTransition;
        [SerializeField] private float transitionDuration;


        private bool _isBlocked;

        public bool IsBlocked
        {
            get => _isBlocked;
            set
            {
                _isBlocked = value;
                _ = FadeEmissive(PillarIsEmpty ? inactiveColor : _isBlocked?blockedColor:activeColor);
            }
        }

        public bool PillarIsEmpty => !_myBall;
        
        public bool CanInteract => !PillarIsEmpty && !IsBlocked;
        
        
        private void OnEnable()
        {
            if(!_material) _material = meshRenderer.material;
            _ = FadeEmissive(PillarIsEmpty ? inactiveColor : activeColor);
        }

        private void OnDisable()
        {
            if(_material) _ = FadeEmissive(inactiveColor);
        }

    

        public void CreateBall(PlayerBallInfo.BallStructure ballInfo)
        {
            _myBall = ResourceManager.CreateBallDisabled(ballInfo.Ball, ballInfo.Weapon, ballPoint.position, ballPoint.rotation);
            foreach(MonoBehaviour nw in _myBall.GetComponentsInChildren<MonoBehaviour>()) nw.enabled = false;
            _myBall.transform.SetParent(transform, true);

            _ = FadeEmissive(activeColor);
        }

        public void RemoveBall()
        {
            Destroy(_myBall.gameObject);
            _ = FadeEmissive(inactiveColor);
        }

        private async UniTask FadeEmissive(Color targetColor)
        {
            Color startColor = _material.GetColor(StaticUtilities.EmissiveID);
            float elapsedTime = 0f;
            
            while (elapsedTime < transitionDuration && _material)
            {
                elapsedTime += Time.deltaTime;
                float t = colorTransition.Evaluate(elapsedTime / transitionDuration);
                _material.SetColor(StaticUtilities.EmissiveID, Color.Lerp(startColor, targetColor, t));
                await UniTask.Yield();
            }
            
            _material?.SetColor(StaticUtilities.EmissiveID, targetColor);
        }

        public void ForceActivate()
        {
            _ = FadeEmissive(activeColor);

        }
    }
}
