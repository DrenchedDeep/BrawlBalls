using System.Collections;
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
    

        public void CreateBall(SaveManager.BallStructure ballInfo)
        {
            
            _myBall = ResourceManager.CreateBallDisabled(ballInfo.Ball, ballInfo.Weapon, ballPoint);
            _myBall.SetAbility(ResourceManager.Abilities[ballInfo.Ability]);
            NetworkObject[] unitySucks = _myBall.GetComponentsInChildren<NetworkObject>();
            foreach (var t in unitySucks)
            {
                Destroy(t);
            }

            _myBall.transform.localScale = Vector3.one;
            
            
            
            Debug.Log("Spawning a ball", _myBall.gameObject);
            
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
            
            while (elapsedTime < transitionDuration )
            {
                elapsedTime += Time.deltaTime;
                float t = colorTransition.Evaluate(elapsedTime / transitionDuration);
                _material.SetColor(StaticUtilities.EmissiveID, Color.Lerp(startColor, targetColor, t));
                await UniTask.Yield();
            }
            
            _material.SetColor(StaticUtilities.EmissiveID, targetColor);
        }

    }
}
