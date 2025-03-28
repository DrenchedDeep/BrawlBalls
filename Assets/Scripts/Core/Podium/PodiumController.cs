using System;
using Cysharp.Threading.Tasks;
using Loading;
using LocalMultiplayer;
using MainMenu.UI;
using Stats;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utilities.Layout;
using Utilities.UI_General;

namespace Core.Podium
{
    [RequireComponent(typeof(PodiumCycleController))]
    public class PodiumController : MonoBehaviour
    {

        [SerializeField] private Podium[] podiums;
        [SerializeField] private Camera cam;
        
        public PlayerInput localPlayerInputComponent;
        [SerializeField]  private EventSystem eventSystem;
        [SerializeField]  private BestVirtualCursor cursor;

        [SerializeField] private InputActionReference previous;
        [SerializeField] private InputActionReference next;
        [SerializeField] private InputActionReference select;
        [SerializeField] private InputActionReference press;

        public UnityEvent<int> onForwardSelected;
        public UnityEvent<int> onSelectedSide;

        public Podium[] Podiums => podiums;
        private Podium _currentPodium;
        public int CurForward { get; set; } = 1;
        
        [NonSerialized] public bool IsRotating;
        


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _ = SpawnBalls();
            _podiumLayer = 1 << gameObject.layer;
        }

        private void OnEnable()
        {
            
            localPlayerInputComponent.actions[previous.name].performed += RotateLeft;
            localPlayerInputComponent.actions[next.name].performed += RotateRight;
            localPlayerInputComponent.actions[select.name].performed += SelectCurrent;
            localPlayerInputComponent.actions[press.name].performed += SelectForwardCursor;
            
            localPlayerInputComponent.SwitchCurrentActionMap("UI");
            
            
        }

        
        
        private void OnDisable()
        {
            previous.action.performed -= RotateLeft;
            next.action.performed -= RotateRight;
            select.action.performed -= SelectCurrent;
            
            localPlayerInputComponent.SwitchCurrentActionMap("Game");

        }

        private int _podiumLayer;

        private void RotateLeft(InputAction.CallbackContext _)
        {
            if (!IsRotating) onSelectedSide.Invoke(-1);
        }

        private void RotateRight(InputAction.CallbackContext _)
        {
            if (!IsRotating) onSelectedSide.Invoke(1);
        }

        private void SelectCurrent(InputAction.CallbackContext _)
        {
            if (!IsRotating)
            {
                onForwardSelected.Invoke(CurForward);
                _currentPodium = podiums[CurForward];

            }
        }

        private async UniTask SpawnBalls()
        {
            await UniTask.WaitUntil(() => !LoadingController.IsLoading);
            await UniTask.Delay(1200);
            try
            {
                Debug.Log("We're spawning these balls!");
                podiums[1].CreateBall(1);
                //podiums[1].ForceActivate();
                await UniTask.Delay(100);
                podiums[0].CreateBall(0);
                //podiums[0].ForceActivate();
                await UniTask.Delay(100);
                podiums[2].CreateBall(2);
                //podiums[2].ForceActivate();
            }

            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void Update()
        {

            if (!eventSystem)
            {
                if (IsRotating || EventSystem.current.IsPointerOverGameObject())
                    return;
                if (!cam)
                {
                
                    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                    Debug.LogError("Why is there no camera?");
                    return;
                }
                Ray l = cam.ScreenPointToRay(Pointer.current.position.ReadValue());
                Debug.DrawRay(l.origin + Vector3.down, l.direction * 1000, Color.red);

                if (Physics.Raycast(l, out RaycastHit k, 1000, _podiumLayer))
                {
                
                    Transform t = k.transform.parent?.parent;
                    if (!t)
                    {
                        SelectPodium(null);
                        return;
                    }


                    if (!t.TryGetComponent(out Podium p))
                    {
                        SelectPodium(null);
                        return;
                    }
                
                    SelectPodium(p);
                }
                else
                {
                    SelectPodium(null);
                }
                return;
            }

            if (IsRotating || eventSystem.IsPointerOverGameObject(localPlayerInputComponent.playerIndex))
                return;

            
            
            
            
            if (!cam)
            {
                
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogError("Why is there no camera?");
                return;
            }
            Ray ray;
            if (!SplitscreenPlayerManager.Instance || SplitscreenPlayerManager.Instance.LocalPlayers.Count <= 1)
            {
                //Debug.Log("Stupid way of checking for mouse position");
                ray = cam.ScreenPointToRay(Pointer.current.position.ReadValue());
            }
            else
            {
                ray = cam.ScreenPointToRay(cursor.Mouse.position.ReadValue());
            }
            
            Debug.DrawRay(ray.origin + Vector3.down, ray.direction * 1000, Color.red);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000, _podiumLayer))
            {
                
                Transform t = hit.transform.parent?.parent;
                if (!t)
                {
                    SelectPodium(null);
                    return;
                }


                if (!t.TryGetComponent(out Podium p))
                {
                    SelectPodium(null);
                    return;
                }
                
                SelectPodium(p);
            }
            else
            {
                SelectPodium(null);
            }
        }

        private void SelectForwardCursor(InputAction.CallbackContext ctx)
        {
            Debug.Log("Pressed on Balls: " + ctx.ReadValueAsButton() +", " + !IsRotating );
            if (!ctx.ReadValueAsButton() || IsRotating) return;
            if (SplitscreenPlayerManager.Instance && SplitscreenPlayerManager.Instance.LocalPlayers.Count > 1)
            {

                //Debug.Log("How many times did I trip? - stupid way of checking for mouse");

                if (_currentPodium)
                {
                    if (_currentPodium == podiums[CurForward] && podiums[CurForward].CanInteract)
                    {
                        onForwardSelected?.Invoke(CurForward);
                        _currentPodium = podiums[CurForward];
                        Debug.Log("Pressed on Center ball");

                        return;
                    }

                    onSelectedSide.Invoke(_currentPodium.transform.localPosition.x >
                                          podiums[CurForward].transform.localPosition.x
                        ? 1
                        : -1);
                    
                    Debug.Log("Pressed on side ball");

                }
                else
                {
                    SelectPodium(null);
                }

                return;
            }
            
            // Debug.Log("Pointer location: " + pointerPosition );
            
            // ReSharper disable once Unity.PerformanceCriticalCodeCameraMain
            if (!cam)
            {
                
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogError("Why is there no camera?");
                return;
            }
            Ray ray = cam.ScreenPointToRay(Pointer.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 1000, _podiumLayer))
            {
                
                Transform t = hit.transform.parent?.parent;


                if (!t || !t.TryGetComponent(out Podium p))
                {
                    Debug.Log("I selected nothing");
                    return;
                }
               
                if (p == podiums[CurForward])
                {
                    if (podiums[CurForward].CanInteract)
                    {        
                        Debug.Log("I selected the forward one!");
                        onForwardSelected?.Invoke(CurForward);
                    }
                    return;
                }
                Debug.Log("I Selected the side one!");
                onSelectedSide.Invoke(t.localPosition.x > podiums[CurForward].transform.localPosition.x ? 1:-1);
            }
            else
            {
                Debug.Log("I selected nothing");
                SelectPodium(null);
            }
            
        }

        private void SelectPodium(Podium podium)
        {
            if (_currentPodium == podium) return;
            if (_currentPodium)
            {
                _currentPodium.StopAllCoroutines();
                _currentPodium.StartCoroutine(_currentPodium.OnStopHover());
            }
            if(podium && podium.CanInteract) podium.StartCoroutine(podium.OnHover());
            _currentPodium = podium;
        }

        public void DisablePodiumAndCycle(int podium)
        {
            podiums[podium].RemoveBall();
        }
        
        public void OnItemSelected(IInfiniteScrollItem item)
        {
            ShopItemStats wheelItem = ((WheelItem)item).GetItem();
            
            //... Validate?

            Podium p = podiums[CurForward];

            if (wheelItem.Stats is WeaponStats)
            {
                p.SetWeapon(wheelItem.Prefab);
            }else if (wheelItem.Stats is AbilityStats st)
            {
                p.SetAbility(st);
            }
            else
            {
                p.SetBall(wheelItem.Prefab);
            }

        }
    }
}
