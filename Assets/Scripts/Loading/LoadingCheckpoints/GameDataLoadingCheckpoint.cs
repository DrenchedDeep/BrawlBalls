using System;
using Cysharp.Threading.Tasks;
using Managers.Local;
using UnityEngine;

namespace Loading.LoadingCheckpoints
{
    public class GameDataLoadingCheckpoint : MonoBehaviour, ILoadingCheckpoint
    {
       
        public Action OnComplete { get; set; }
        public Action OnFailed { get; set; }
        public async UniTask Execute()
        {
            Debug.Log("Beginning loading checkpoint", gameObject);

            SaveManager.MyBalls = await SaveManager.LoadData();
        }

        public bool IsCompleted()
        {
            return SaveManager.MyBalls != null;
        }
    }
    

}
