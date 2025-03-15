using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Balls;
using Gameplay.Weapons;
using Managers.Local;
using Stats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Podium
{
    public class Podium : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Transform ballPoint;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image abilityIcon;
     
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

        private int _ballIndex = -1;
        private GameObject _ballObject;
        private GameObject _weaponObject;
        private Material _ballMaterial;
        private Material[] _weaponMaterial;


        private AudioSource _audioSource;

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

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            canvas.enabled = false;
        }

        private void OnEnable()
        {

            if(!_material) _material = meshRenderer.material;
            _ = FadeEmissive(PillarIsEmpty ? inactiveColor : activeColor);
        }
    

        public void CreateBall(int index)
        {
            _audioSource.Play();
            _audioSource.time = 0.1f;
            _ballIndex = index;
            SaveManager.BallStructure ballInfo = SaveManager.MyBalls.GetReadonlyBall(index);
            _myBall = ResourceManager.CreateBallDisabled(ballInfo.ball, ballInfo.weapon, ballPoint, out var b, out var w);
            AbilityStats stats = ResourceManager.Abilities[ballInfo.ability];
            abilityIcon.sprite = stats.Icon;

            canvas.enabled = true;
            
            _myBall.SetAbility(stats);
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
            
            
            NetworkObject[] unitySucks = _myBall.GetComponentsInChildren<NetworkObject>();
            
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
           canvas.enabled = false;
        }

        private async UniTask FadeEmissive(Color targetColor)
        {
            Color startColor = _material.GetColor(StaticUtilities.EmissiveID);
            float elapsedTime = 0f;
            
            while (elapsedTime < transitionDuration )
            {
                elapsedTime += Time.deltaTime;
                float t = colorTransition.Evaluate(elapsedTime / transitionDuration);
                _material.SetColor(StaticUtilities.EmissiveID, Color.LerpUnclamped(startColor, targetColor, t));
                await UniTask.Yield();
            }
            
            _material.SetColor(StaticUtilities.EmissiveID, targetColor);
        }

        public void SetWeapon(GameObject w)
        {
            Debug.Log($"Changing the WEAPON from {SaveManager.MyBalls.GetReadonlyBall(_ballIndex).ball} to {w.name}");
            SaveManager.MyBalls.SetBallWeapon(_ballIndex,  w.name);
            
            Destroy(_weaponObject);

            _weaponObject = Instantiate(w, ballPoint);



            
            Destroy(_weaponObject.GetComponentInChildren<NetworkObject>());

            _weaponObject.GetComponentInChildren<BaseWeapon>().enabled = false;
            
            
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
            Debug.Log($"Changing the BALL from {SaveManager.MyBalls.GetReadonlyBall(_ballIndex).ball} to {b.name}");
            SaveManager.MyBalls.SetBallType(_ballIndex, b.name);
            
            Destroy(_ballObject);
            
            _ballObject = Instantiate(b, ballPoint);
            _ballObject.GetComponentInChildren<Ball>().enabled = false;

            Destroy(_ballObject.GetComponentInChildren<NetworkObject>());

            _ballMaterial = _ballObject.GetComponent<MeshRenderer>().material;
            _ = TransitionMaterial(_ballMaterial, StaticUtilities.FlashPercentID, 1,0);
            
        }

        public void SetAbility(AbilityStats a)
        {
            Debug.Log($"Changing the ABILITY from {SaveManager.MyBalls.GetReadonlyBall(_ballIndex).ability} to {a.name}");
            SaveManager.MyBalls.SetBallAbility(_ballIndex, a.name);
            _ = TransitionMaterial(_ballMaterial, StaticUtilities.FlashPercentID, 1,0);

            abilityIcon.sprite = a.Icon;
            canvas.enabled = true;
        }

        private async UniTask TransitionMaterial(Material m, int shaderID, float start, float end)
        {
            float t = 0;
            while (t < flashDuration)
            {
                t += Time.deltaTime;
                float p = flashTransition.Evaluate(t / flashDuration);
                
                m.SetFloat(shaderID, Mathf.LerpUnclamped(start,end,p));
                
                await UniTask.Yield();
            }
            m.SetFloat(shaderID, end);
        }


    }
}
