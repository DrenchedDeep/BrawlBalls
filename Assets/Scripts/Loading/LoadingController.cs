using System;
using System.Collections.Generic;
using UnityEngine;

namespace Loading
{
    public class LoadingController : MonoBehaviour
    {
        private int _numNeeded;
        private int _numCompleted;

        [SerializeField] private bool loadOnAwake;
        private void Awake()
        {
            if(loadOnAwake) BeginLoading();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public async void BeginLoading()
        {
            LoadingHelper.Instance.Activate();
            ILoadingCheckpoint[] checkpoints = GetComponents<ILoadingCheckpoint>();
            _numNeeded = checkpoints.Length + 1;
            LoadingHelper.Instance.SetProgress(0);

            List<ILoadingCheckpoint> waitForComplete = new();
            foreach (ILoadingCheckpoint checkpoint in checkpoints)
            {
                if (!checkpoint.IsCompleted())
                {
                    waitForComplete.Add(checkpoint);
                }
            }

            if (waitForComplete.Count == 0)
            {
                Debug.Log("Loading is already complete, we don't need to do anything more! :)");
                return;
            }

            
            ItemComplete();
            
            foreach (ILoadingCheckpoint checkpoint in waitForComplete)
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
