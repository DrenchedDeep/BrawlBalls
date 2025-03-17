using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Loading.LoadingCheckpoints
{
    public class MusicTransitionCheckpoint : MonoBehaviour, ILoadingCheckpoint
    {
        //Stream music in with addressables...?
        //We need to access the audio manager...?
        
        public Action OnComplete { get; set; }
        public Action OnFailed { get; set; }
        public UniTask Execute()
        {
            return UniTask.CompletedTask;
        }

        public bool IsCompleted()
        {
            Debug.LogWarning("MusicTransitionCheckpoint: IS NOT IMPLEMENTED");
            return true;
        }
    }
}
