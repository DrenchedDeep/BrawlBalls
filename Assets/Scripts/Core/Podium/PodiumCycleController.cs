using Cysharp.Threading.Tasks;
using Managers.Local;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Core.Podium
{
    public class PodiumCycleController : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float duration = 1;
        
        [SerializeField] private Podium [] podiums;
        public UnityEvent<int> onForwardSelected;
        
        [Header("ClusterRotation")]
        [SerializeField] private Transform camRotator;
        [SerializeField] private Transform camRotatorStart;
        [SerializeField] private Transform camRotatorEnd;
        [SerializeField] private Transform podiumRoot;
        [SerializeField] private Transform podiumRootStart;
        [SerializeField] private Transform podiumRootEnd;
        [SerializeField] private float transitionTime = 0.3f;

        [SerializeField] private bool debugHide;
        
        private bool _isRotating;
        private int _curForward = 1;
        private bool _isLowering;
        private bool _inCustomization;
        private bool _isSide;


        private void Start()
        {
            if (debugHide)
            {
                Debug.LogWarning("Fix intentional garbage", gameObject);
                for (int i = 0; i < podiums.Length; i++)
                {
                    podiums[i].ForceActivate();

                }
                
                return;
            }
            for (int i = 0; i < podiums.Length; i++)
            {
                podiums[i].CreateBall(PlayerBallInfo.Balls[i]);
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
            if(!cam) cam = Camera.main;
            if (!cam)
            {
                Debug.LogWarning("Why is there no camera?");
                return;
            }
            Ray ray = cam.ScreenPointToRay(pointerPosition);
            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 3);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000, StaticUtilities.PodiumBlockers))
            {
                Debug.LogWarning(" I did hit something: ", hit.transform.gameObject);
                Transform t = hit.transform.parent;
                if (!t) return;
                if (t == podiums[_curForward].transform)
                {
                    Debug.Log($"{debugHide} || {podiums[_curForward].CanInteract}");
                    if(debugHide || podiums[_curForward].CanInteract)
                        onForwardSelected?.Invoke(_curForward);
                    return;
                }
                Move(hit.transform.parent.localPosition.x > podiums[_curForward].transform.localPosition.x ? 1:-1);
            }
        }

        private void Move(int dir)
        {
            if (_isRotating) return;
            int start = _curForward;
            Vector3 prv = podiums[start].transform.position;
            Quaternion prvRot = podiums[start].transform.rotation;
            _isRotating = true;
            print("rotating: " + dir);
            do
            {
                start += dir;
                if (start < 0) start = podiums.Length - 1;
                else if (start ==  podiums.Length) start = 0;
                Vector3 temp = podiums[start].transform.position;
                Quaternion prvRotTemp = podiums[start].transform.rotation;
                _ = SlerpIt(prv, prvRot, podiums[start].transform, duration);
                prv = temp;
                prvRot = prvRotTemp;
            } while (_curForward != start);
            _curForward += dir;
            if (_curForward < 0) _curForward = podiums.Length - 1;
            else if (_curForward ==  podiums.Length) _curForward = 0;
        }

        private async UniTask SlerpIt(Vector3 next, Quaternion rotation, Transform id, float time)
        {
            float curTime = 0;
            Vector3 origin = id.position;
            Quaternion rot = id.rotation;
            while (curTime < time)
            {
                curTime += Time.deltaTime;
                float t = curTime / time;
                id.SetPositionAndRotation(Vector3.Slerp(origin, next, t), Quaternion.Slerp(rot, rotation, t));
                await UniTask.Yield();
            }

            _isRotating = false;
        }

        public void ToggleViewState()
        {
            _isSide = !_isSide;

            if (_isSide)
            {
                _ = SlerpIt(podiumRootEnd.position, podiumRootEnd.rotation, podiumRoot,transitionTime);
                _ = SlerpIt(camRotatorEnd.position, camRotatorEnd.rotation, camRotator,transitionTime);
            }
            else
            {
                _ = SlerpIt(podiumRootStart.position, podiumRootStart.rotation, podiumRoot,transitionTime);
                _ = SlerpIt(camRotatorStart.position, camRotatorStart.rotation, camRotator,transitionTime);
            }
        }

        public void DisablePodiumAndCycle(int podium)
        {
            podiums[podium].RemoveBall();
        }

    }
}
