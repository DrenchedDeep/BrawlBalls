using System;
using Cysharp.Threading.Tasks;
using Loading;
using MainMenu.UI;
using Managers.Local;
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

        public UnityEvent<int> onForwardSelected;
        public UnityEvent<int> onSelectedSide;

        public Podium[] Podiums => podiums;

        public int CurForwarad { get; set; } = 1;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _ = SpawnBalls();
        }

        private async UniTask SpawnBalls()
        {
            await UniTask.WaitUntil(() => !LoadingController.IsLoading);
            await UniTask.Delay(1200);
            try
            {
                Debug.Log("We're spawning these balls!");
                podiums[1].CreateBall(SaveManager.MyBalls[1]);
                //podiums[1].ForceActivate();
                await UniTask.Delay(100);
                podiums[0].CreateBall(SaveManager.MyBalls[0]);
                //podiums[0].ForceActivate();
                await UniTask.Delay(100);
                podiums[2].CreateBall(SaveManager.MyBalls[2]);
                //podiums[2].ForceActivate();
            }

            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void Update()
        {
            
            
            if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
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
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 3);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000, StaticUtilities.PodiumBlockers))
            {
                Debug.LogWarning(" I did hit something: ", hit.transform.gameObject);
                Transform t = hit.transform.parent;
                if (!t) return;
                if (t == podiums[CurForwarad].transform)
                {
                    Debug.Log($"{debugHide} || {podiums[CurForwarad].CanInteract}");
                    if (debugHide || podiums[CurForwarad].CanInteract)
                    {        
                        Debug.Log("Remember to disable the object if we're updating still!");
                        onForwardSelected?.Invoke(CurForwarad);
                    }
                    return;
                }
                onSelectedSide.Invoke(hit.transform.parent.localPosition.x > podiums[CurForwarad].transform.localPosition.x ? 1:-1);
            }
        }
        
        public void DisablePodiumAndCycle(int podium)
        {
            podiums[podium].RemoveBall();
        }
        
        public void OnItemSelected(IInfiniteScrollItem item)
        {
            ShopItemStats wheelItem = ((WheelItem)item).GetItem();
            
            //... Validate?

            Podium p = podiums[CurForwarad];

            if (wheelItem.Stats is WeaponStats ws)
            {
                p.SetWeapon(wheelItem.Prefab);
            }else if (wheelItem.Stats is BallStats bs)
            {
                p.SetBall(wheelItem.Prefab);
            }
            else
            {
                p.SetAbility(null);
            }

        }
    }
}
