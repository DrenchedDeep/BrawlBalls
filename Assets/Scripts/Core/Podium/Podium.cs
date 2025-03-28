using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Balls;
using Gameplay.Weapons;
using Loading;
using Managers.Local;
using Stats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Core.Podium
{
    [DefaultExecutionOrder(100)]
    public class Podium : MonoBehaviour, ISelectHandler, IDeselectHandler
        {
            [SerializeField] private MeshRenderer meshRenderer;
            [SerializeField] private Transform ballPoint;
            [SerializeField] private Canvas canvas;
            [SerializeField] private Image abilityIcon;
         
            private Material _material;
            private BallPlayer _myBall;

            [Header("HoverTransition Transition")]
            [SerializeField] private AnimationCurve hoverTransitionCurve;
            [SerializeField] private float hoverTransitionTime;
            [SerializeField] private Vector3 localHoverOffset;
            [SerializeField] private Vector3 localHoverScale;
            private float _currentHoverTransitionTime;
            private Vector3 _originalHoverOffset;
            private Vector3 _originalHoverScale;
            private Transform _core;
            
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
            private SaveManager.PlayerData _myPlayerData;
            

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

            private IEnumerator Start()
            {
                canvas.enabled = false;
                _core = transform.GetChild(0);
                _originalHoverOffset = _core.localPosition;
                _originalHoverScale = _core.localScale;
                var playerInput = transform.root.GetComponent<PlayerInput>();
                yield return new WaitWhile(() => LoadingController.IsLoading);
                if (!SaveManager.TryGetPlayerData(playerInput, out _myPlayerData))
                {
                    Debug.LogError("SOMEHOW WE LOADED A PLAYER WHO HAS NO DATA???", gameObject);
                }
            }

            private void OnEnable()
            {
                if(!_material) _material = meshRenderer.material;
                _ = FadeEmissive(PillarIsEmpty ? inactiveColor : activeColor);
            }

            private bool Ready() => _myPlayerData != null;
            
            public async void CreateBall(int index)
            {

                await UniTask.WaitUntil(Ready);
                
                //_audioSource.Play();
                //_audioSource.time = 0.1f;
                
                AudioManager.instance.PlayOneShot(FMODEvents.instance.spawnBall[0], transform.position);
                //Debug.LogError("Tried to play audio, carlos please fix -- Add ball spawn effect");
                
                _ballIndex = index;
                SaveManager.BallStructure ballInfo = _myPlayerData.GetReadonlyBall(index);
                _myBall = ResourceManager.CreateBallDisabled(ballInfo.ball, ballInfo.weapon, ballPoint, out var b, out var w);
                AbilityStats stats = ResourceManager.Abilities[ballInfo.ability];
                abilityIcon.sprite = stats.Icon;

                canvas.enabled = true;
                
                _myBall.SetAbility(stats);
                TrailRenderer trail = _myBall.GetComponentInChildren<TrailRenderer>();

                _ballObject = b.gameObject;
                _weaponObject = w.gameObject;
                
                
         
                
                _ballMaterial = b.GetComponent<MeshRenderer>().material;
                _ = TransitionMaterial(_ballMaterial, StaticUtilities.AppearPercentID, 1,0);

                MeshRenderer[] mesh = w.GetComponentsInChildren<MeshRenderer>();
                _weaponMaterial = new Material[mesh.Length];
                for (int i = 0; i < mesh.Length; i++)
                {
                    _weaponMaterial[i] = mesh[i].material;
                    mesh[i].gameObject.layer = gameObject.layer;
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
                
                _ballObject.layer = gameObject.layer;
                _weaponObject.layer = gameObject.layer;
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
                AudioManager.instance.PlayOneShot(FMODEvents.instance.click, transform.position);
                Debug.Log($"Changing the WEAPON from {_myPlayerData.GetReadonlyBall(_ballIndex).ball} to {w.name}");
                _myPlayerData.SetBallWeapon(_ballIndex,  w.name);
                
                Destroy(_weaponObject);

                _weaponObject = Instantiate(w, ballPoint);


                
                Destroy(_weaponObject.GetComponentInChildren<NetworkObject>());

                _weaponObject.GetComponentInChildren<BaseWeapon>().enabled = false;
                
                
                MeshRenderer[] mesh = _weaponObject.GetComponentsInChildren<MeshRenderer>();
                _weaponMaterial = new Material[mesh.Length];
                for (int i = 0; i < mesh.Length; i++)
                {
                    _weaponMaterial[i] = mesh[i].material;
                    mesh[i].gameObject.layer = gameObject.layer;
                    _ = TransitionMaterial(_weaponMaterial[i], StaticUtilities.FlashPercentID,1,0);
                }
                _weaponObject.layer = gameObject.layer;

            }

            public void SetBall(GameObject b)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.click, transform.position);
                Debug.Log($"Changing the BALL from {_myPlayerData.GetReadonlyBall(_ballIndex).ball} to {b.name}");
                _myPlayerData.SetBallType(_ballIndex, b.name);
                Destroy(_ballObject);
                
                _ballObject = Instantiate(b, ballPoint);
                _ballObject.GetComponentInChildren<Ball>().enabled = false;

                Destroy(_ballObject.GetComponentInChildren<NetworkObject>());

                _ballMaterial = _ballObject.GetComponent<MeshRenderer>().material;
                _ = TransitionMaterial(_ballMaterial, StaticUtilities.FlashPercentID, 1,0);
                
                _ballObject.layer = gameObject.layer;

            }

            public void SetAbility(AbilityStats a)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.click, transform.position);
                Debug.Log($"Changing the ABILITY from {_myPlayerData.GetReadonlyBall(_ballIndex).ability} to {a.name}");
                _myPlayerData.SetBallAbility(_ballIndex, a.name);
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


            public IEnumerator OnHover()
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.scroll, transform.position);
                
                while (_currentHoverTransitionTime <= hoverTransitionTime)
                {
                    _currentHoverTransitionTime += Time.deltaTime;
                    float progress = Mathf.Clamp01(_currentHoverTransitionTime / hoverTransitionTime);
                    float curveValue = hoverTransitionCurve.Evaluate(progress);
                    _core.localPosition = Vector3.LerpUnclamped(_originalHoverOffset, localHoverOffset, curveValue);
                    _core.localScale = Vector3.LerpUnclamped(_originalHoverScale, localHoverScale, curveValue);
                    yield return null;
                }
                // Ensure the final state is set.
                _core.localPosition = localHoverOffset;
                _core.localScale = localHoverScale;
                _currentHoverTransitionTime = hoverTransitionTime;
                
            }

            public IEnumerator OnStopHover()
            {
                while (_currentHoverTransitionTime > 0)
                {
                    _currentHoverTransitionTime -= Time.deltaTime;
                    float progress = Mathf.Clamp01(_currentHoverTransitionTime / hoverTransitionTime);
                    float curveValue = hoverTransitionCurve.Evaluate(progress);
                    _core.localPosition = Vector3.LerpUnclamped(_originalHoverOffset, localHoverOffset, curveValue);
                    _core.localScale = Vector3.LerpUnclamped(_originalHoverScale, localHoverScale, curveValue);
                    yield return null;
                }
                // Ensure the final state is set.
                _core.localPosition = _originalHoverOffset;
                _core.localScale = _originalHoverScale;
                _currentHoverTransitionTime = 0;
            }

            public void OnSelect(BaseEventData eventData)
            {
                AudioManager.instance.PlayOneShot(FMODEvents.instance.scroll, transform.position);
            }

            public void OnDeselect(BaseEventData eventData)
            {
                //RuntimeManager.PlayOneShot();
            }
        }
}
