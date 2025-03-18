using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Podium
{
    [RequireComponent(typeof(PodiumCycleController))]
    public class PodiumCycleController : MonoBehaviour
    {
        [SerializeField] private float duration = 1;
        
        [Header("ClusterRotation")]
        [SerializeField] private Transform camRotator;
        [SerializeField] private Transform camRotatorStart;
        [SerializeField] private Transform camRotatorEnd;
        [SerializeField] private Transform podiumRoot;
        [SerializeField] private Transform podiumRootStart;
        [SerializeField] private Transform podiumRootEnd;
        [SerializeField] private float transitionTime = 0.3f;


        
        
        private bool _isLowering;
        private bool _inCustomization;
        private bool _isSide;
        
        private PodiumController _podiumController;

        private void Awake()
        {
            _podiumController = GetComponent<PodiumController>();

        }

        public void Move(int dir)
        {
            
            if (_podiumController.IsRotating) return;
            
            int start = _podiumController.CurForward;
            Transform tr = _podiumController.Podiums[start].transform;
            Vector3 prv =tr.position;
            Quaternion prvRot = tr.rotation;
            _podiumController.IsRotating = true;
            print("rotating: " + dir);
            do
            {
                start += dir;
                if (start < 0) start = _podiumController.Podiums.Length - 1;
                else if (start ==  _podiumController.Podiums.Length) start = 0;
                tr = _podiumController.Podiums[start].transform;
                Vector3 temp = tr.position;
                Quaternion prvRotTemp = tr.rotation;
                _ = SlerpIt(prv, prvRot, tr.transform, duration);
                prv = temp;
                prvRot = prvRotTemp;
            } while (_podiumController.CurForward != start);
            _podiumController.CurForward += dir;
            if (_podiumController.CurForward < 0) _podiumController.CurForward = _podiumController.Podiums.Length - 1;
            else if (_podiumController.CurForward ==  _podiumController.Podiums.Length) _podiumController.CurForward = 0;
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
            id.SetPositionAndRotation(next, rotation);

            _podiumController.IsRotating = false;
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

      

    }
}
