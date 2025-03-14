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
        
        [Header("Creation")]
        [SerializeField] private AnimationCurve flashTransition;
        [SerializeField] private float flashDuration = 0.5f;

        private bool _isBlocked;

        private GameObject _ballObject;
        private GameObject _weaponObject;
        private Material _ballMaterial;
        private Material[] _weaponMaterial;
        

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
            
            _myBall = ResourceManager.CreateBallDisabled(ballInfo.Ball, ballInfo.Weapon, ballPoint, out var b, out var w);
            _myBall.SetAbility(ResourceManager.Abilities[ballInfo.Ability]);
            NetworkObject[] unitySucks = _myBall.GetComponentsInChildren<NetworkObject>();
            TrailRenderer trail = _myBall.GetComponent<TrailRenderer>();

            _ballObject = b.gameObject;
            _weaponObject = w.gameObject;
            
            _ballMaterial = b.GetComponent<MeshRenderer>().material;
            _ = TransitionMaterial(_ballMaterial, StaticUtilities.AppearPercentID, 1,0);

            MeshRenderer[] mesh = w.GetComponentsInChildren<MeshRenderer>();
            _weaponMaterial = new Material[mesh.Length];
            for (int i = 0; i < mesh.Length; i++)
            {
                _weaponMaterial[i] = mesh[i].material;
                _ = TransitionMaterial(_weaponMaterial[i], StaticUtilities.FlashPercentID,1,0);
            }
            
            Destroy(trail);
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

        public void SetWeapon(GameObject w)
        {
            Destroy(_weaponObject);

            _weaponObject = Instantiate(w, ballPoint);
            MeshRenderer[] mesh = _weaponObject.GetComponentsInChildren<MeshRenderer>();
            _weaponMaterial = new Material[mesh.Length];
            for (int i = 0; i < mesh.Length; i++)
            {
                _weaponMaterial[i] = mesh[i].material;
                _ = TransitionMaterial(_weaponMaterial[i], StaticUtilities.FlashPercentID,1,0);
            }

        }

        public void SetBall(GameObject b)
        {
            
        }

        public void SetAbility(Ability a)
        {
           
        }

        private async UniTask TransitionMaterial(Material m, int shaderID, float start, float end)
        {
            float t = 0;
            while (t < flashDuration)
            {
                t += Time.deltaTime;
                float p = flashTransition.Evaluate(t / flashDuration);
                
                m.SetFloat(shaderID, Mathf.Lerp(start,end,p));
                
                await UniTask.Yield();
            }
            m.SetFloat(shaderID, end);
        }


    }
}
