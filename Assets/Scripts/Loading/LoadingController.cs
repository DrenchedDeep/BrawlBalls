using MainMenu.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Loading
{
    public class LoadingController : MonoBehaviour
    {
        private int _numNeeded;
        private int _numCompleted;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public async void BeginLoading()
        {
            LoadingHelper.Instance.Activate();
            ILoadingCheckpoint[] checkpoints = GetComponents<ILoadingCheckpoint>();
            _numNeeded = checkpoints.Length;
            LoadingHelper.Instance.SetProgress(0);
            foreach (ILoadingCheckpoint checkpoint in checkpoints)
            {
                checkpoint.OnComplete += ItemComplete;
                checkpoint.OnFailed += ItemFailed;
                await checkpoint.Execute();
            }
            LoadingHelper.Instance.Deactivate();
        }

        private void ItemFailed()
        {
            Debug.LogError("FAILED TO LOAD");
            LoadingHelper.Instance.SetText("Failed to load, reason unknown - Please contact support", 10);
        }

        private void ItemComplete()
        {
            _numCompleted += 1;
            LoadingHelper.Instance.SetProgress( (float)_numCompleted / _numNeeded);
        }
    }
}
