using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Loading.LoadingCheckpoints
{
    public class SceneLoadingCheckpoint : MonoBehaviour, ILoadingCheckpoint
    {
        [SerializeField] private int sceneIndex = 1;
        [SerializeField] private LoadSceneMode loadSceneMode = LoadSceneMode.Additive;
        public async UniTask Execute()
        {
            AsyncOperation op;
            Scene previousScene = SceneManager.GetActiveScene();
            try
            {
                op = SceneManager.LoadSceneAsync(sceneIndex, loadSceneMode);
                
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                OnFailed?.Invoke();
                return;
            }

            await op.ToUniTask();

            if (loadSceneMode is LoadSceneMode.Additive)
            {
                op = SceneManager.UnloadSceneAsync(previousScene);
                
                await op.ToUniTask();

            }
                
            OnComplete.Invoke();
        }

        public Action OnComplete { get; set; }
        public Action OnFailed { get; set; }

    }
}
