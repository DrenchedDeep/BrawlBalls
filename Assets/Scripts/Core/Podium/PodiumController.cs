using System;
using Cysharp.Threading.Tasks;
using Loading;
using MainMenu.UI;
using Stats;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utilities.Layout;

namespace Core.Podium
{
    [RequireComponent(typeof(PodiumCycleController))]
    public class PodiumController : MonoBehaviour
    {

        [SerializeField] private Podium[] podiums;
        [SerializeField] private Camera cam;
        [SerializeField] private bool debugHide;
        
        [SerializeField] private PlayerInput localPlayerInputComponent;
        [SerializeField] private InputActionReference previous;
        [SerializeField] private InputActionReference next;
        [SerializeField] private InputActionReference select;

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
        }

        private void OnDisable()
        {
            previous.action.performed -= RotateLeft;
            next.action.performed -= RotateRight;
            select.action.performed -= SelectCurrent;
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
            if (!IsRotating) onForwardSelected.Invoke(CurForward);
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
            
            
            if (Pointer.current == null || IsRotating)
                return;
            
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return; // Block the raycast if UI is in the way
            }

            

            Vector2 pointerPosition = Pointer.current.position.ReadValue();
            // ReSharper disable once Unity.PerformanceCriticalCodeCameraMain
            if(!cam) cam = Camera.main;
            if (!cam)
            {
                
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogError("Why is there no camera?");
                return;
            }

            Ray ray = cam.ScreenPointToRay(pointerPosition);
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
                

                if (!Pointer.current.press.wasPressedThisFrame) return;
               
                if (p == podiums[CurForward])
                {
                    Debug.Log($"{debugHide} || {podiums[CurForward].CanInteract}");
                    if (debugHide || podiums[CurForward].CanInteract)
                    {        
                        Debug.Log("Remember to disable the object if we're updating still!");
                        onForwardSelected?.Invoke(CurForward);
                    }
                    return;
                }
                onSelectedSide.Invoke(hit.transform.parent.localPosition.x > podiums[CurForward].transform.localPosition.x ? 1:-1);
                
            }
            else
            {
                SelectPodium(null);
            }
        }

        private void SelectPodium(Podium podium)
        {
            if (_currentPodium == podium) return;
            _currentPodium?.OnStopHover();
            podium?.OnHover();
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
