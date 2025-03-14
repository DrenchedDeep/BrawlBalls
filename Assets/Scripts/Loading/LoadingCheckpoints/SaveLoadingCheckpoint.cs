using System;
using Cysharp.Threading.Tasks;
using Managers.Local;
using UnityEngine;

namespace Loading.LoadingCheckpoints
{
    public class SaveLoadingCheckpoint : MonoBehaviour, ILoadingCheckpoint
    {
        public Action OnComplete { get; set; }
        public Action OnFailed { get; set; }
        public async UniTask Execute()
        {
            try
            {
                await SaveManager.MyBalls.SaveData();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                OnFailed?.Invoke();
            }
            OnComplete?.Invoke();
        }

        public bool IsCompleted()
        {
            return SaveManager.MyBalls != null && !SaveManager.MyBalls.HasChanges();
        }
    }
}
