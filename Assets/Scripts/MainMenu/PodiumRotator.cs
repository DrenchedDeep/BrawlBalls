using Cysharp.Threading.Tasks;
using Managers.Local;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace MainMenu
{
    public class PodiumRotator : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float duration = 1;
        [SerializeField] private Transform [] podiums;
        [SerializeField] private Transform camRotator;
        private bool _isRotating;
        private int _curForward = 1;
        private bool _isLowering;
        private bool _inCustomization;

        [SerializeField] private UnityEvent onForwardSelected;
        
        
        
        private void Update()
        {
            if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
                return;

            Vector2 pointerPosition = Pointer.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(pointerPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000, StaticUtilities.PodiumLayer))
            {
                if (hit.transform.parent == podiums[_curForward])
                {
                    Debug.Log("Hit forward one");
                    onForwardSelected?.Invoke();
                    return;
                }

                Move(hit.transform.parent.localPosition.x > podiums[_curForward].localPosition.x ? 1:-1);
            }
        }

        private void Move(int dir)
        {
            if (_isRotating) return;
            int start = _curForward;
            Vector3 prv = podiums[start].position;
            Quaternion prvRot = podiums[start].rotation;
            _isRotating = true;
            print("rotating: " + dir);
            do
            {
                start += dir;
                if (start < 0) start = podiums.Length - 1;
                else if (start ==  podiums.Length) start = 0;
                Vector3 temp = podiums[start].position;
                Quaternion prvRotTemp = podiums[start].rotation;
                _ = SlerpIt(prv, prvRot, podiums[start]);
                prv = temp;
                prvRot = prvRotTemp;
            } while (_curForward != start);
            _curForward += dir;
            if (_curForward < 0) _curForward = podiums.Length - 1;
            else if (_curForward ==  podiums.Length) _curForward = 0;
        }

        private async UniTask SlerpIt(Vector3 next, Quaternion rotation, Transform id)
        {
            float curTime = 0;
            Vector3 origin = id.position;
            Quaternion rot = id.rotation;
            while (curTime < duration)
            {
                curTime += Time.deltaTime;
                float t = curTime / duration;
                id.SetPositionAndRotation(Vector3.Slerp(origin, next, t), Quaternion.Slerp(rot, rotation, t));
                await UniTask.Yield();
            }

            _isRotating = false;
        }

    }
}
