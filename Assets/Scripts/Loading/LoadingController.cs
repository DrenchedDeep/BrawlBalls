using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Loading
{
    [DefaultExecutionOrder(-150)]
    public class LoadingController : MonoBehaviour
    {
        private int _numCompleted;

        [SerializeField] private bool loadOnAwake;
        public static bool IsLoading { get; private set; }
        
        public static LoadingController Instance { get; private set; }
        private readonly List<ILoadingCheckpoint> _loadingCheckpoints = new();
        private void OnEnable()
        {
            if (Instance && Instance != this)
            {
                Destroy(Instance);
                return;
            }

            Instance = this;

            foreach (var checkpoint in GetComponents<ILoadingCheckpoint>() )
            {
                RegisterLoadingComponent(checkpoint);
            }
        }

        private void Start()
        {
            if(loadOnAwake)  BeginLoading();
        }

        public void RegisterLoadingComponent(ILoadingCheckpoint loadingComponent)
        {
            Debug.Log("Registering component for load: " + (MonoBehaviour)loadingComponent, (MonoBehaviour)(loadingComponent));

            loadingComponent.OnComplete += ItemComplete;
            loadingComponent.OnFailed += ItemFailed;
            _loadingCheckpoints.Add(loadingComponent);
        }
        

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public async void BeginLoading()
        {
            if (IsLoading) return;
            
            IsLoading = true;
            LoadingHelper.Instance.Activate();
            LoadingHelper.Instance.SetProgress(0);
            _numCompleted = -1; // Start at negative because we add when doing item complete.
            
            List<ILoadingCheckpoint> waitForComplete = new();
            for (var index = _loadingCheckpoints.Count - 1; index >= 0; index--)
            {
                var checkpoint = _loadingCheckpoints[index];
                
                if (checkpoint == null)
                {
                    _loadingCheckpoints.RemoveAt(index);
                    continue;
                }
               
                if (!checkpoint.IsCompleted())
                {
                    waitForComplete.Add(checkpoint);
                }
                else
                {
                    _numCompleted += 1;
                }
            }

            if (waitForComplete.Count == 0)
            {
                Debug.Log("Loading is already complete, we don't need to do anything more! :)");
                EndLoading();
                return;
            }

            ItemComplete();

            for (var index = waitForComplete.Count - 1; index >= 0; index--)
            {
                var checkpoint = waitForComplete[index];
                Debug.Log("We are now loading: " + (MonoBehaviour)checkpoint, (MonoBehaviour)(checkpoint));
                await checkpoint.Execute();
            }

            EndLoading();

        }


        private void EndLoading()
        {
            LoadingHelper.Instance.Deactivate();
            IsLoading = false;
        }

        private void ItemFailed()
        {
            Debug.LogError("FAILED TO LOAD");
            LoadingHelper.Instance.SetText("Failed to load, reason unknown - Please contact support", 10);
        }

        private void ItemComplete()
        {
            Debug.Log("Completed an item! " + ((float)(1+_numCompleted) / _loadingCheckpoints.Count));
            LoadingHelper.Instance.SetProgress( (float)++_numCompleted / _loadingCheckpoints.Count);
        }

    }
}
