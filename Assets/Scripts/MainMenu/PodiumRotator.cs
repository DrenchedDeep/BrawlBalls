using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MainMenu
{
    public class PodiumRotator : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float duration = 1;
        
        private Transform [] _transPositions;
        private bool _isRotating;
        private int _curForward = 1;
        private bool _isLowering;
        private bool _inCustomization;
    
        // Start is called before the first frame update
        void Start()
        {
            int n = transform.childCount;
            _transPositions = new Transform[n];
            print($"There are {n} podium transforms");
            for (int i = 0; i < n; ++i)
            {
                _transPositions[i] = transform.GetChild(i);
            }
        }
    
        private void Update()
        {
            if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
                return;

            Vector2 pointerPosition = Pointer.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(pointerPosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Move(hit.point.x > 0 ? 1:-1);
            }
        }

        private void Move(int dir)
        {
            if (_isRotating) return;
            int start = _curForward;
            Vector3 prv = _transPositions[start].position;
            _isRotating = true;
            print("rotating: " + dir);
            do
            {
                start += dir;
                if (start < 0) start = _transPositions.Length - 1;
                else if (start ==  _transPositions.Length) start = 0;
                Vector3 temp = _transPositions[start].position;
                _ = SlerpIt(prv, _transPositions[start]);
                prv = temp;
            } while (_curForward != start);
            _curForward += dir;
            if (_curForward < 0) _curForward = _transPositions.Length - 1;
            else if (_curForward ==  _transPositions.Length) _curForward = 0;
        }

        private async UniTask SlerpIt(Vector3 next, Transform id)
        {
            float curTime = 0;
            Vector3 origin = id.position;
            while (curTime < duration)
            {
                curTime += Time.deltaTime;
                id.position = Vector3.Slerp(origin, next, curTime / duration);
                await UniTask.Yield();
            }

            _isRotating = false;
        }

    }
}
