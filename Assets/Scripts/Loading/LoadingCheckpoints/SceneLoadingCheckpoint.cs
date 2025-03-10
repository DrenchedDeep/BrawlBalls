using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Loading.LoadingCheckpoints
{
    public class SceneLoadingCheckpoint : MonoBehaviour, ILoadingCheckpoint
    {
        [SerializeField] private int sceneIndex = 1;
        public async UniTask Execute()
        {
            AsyncOperation op;
            try
            {
                op = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                OnFailed?.Invoke();
                return;
            }

            await op.ToUniTask();
                
            OnComplete.Invoke();
        }

        public Action OnComplete { get; set; }
        public Action OnFailed { get; set; }

    }
}
