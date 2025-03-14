using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Map
{
    public class SpawnPoint : MonoBehaviour
    {
        public static SpawnPoint CurrentSpawnPoint { get; private set; }
        private Material _myMaterial;
        private static readonly int AmountID = Shader.PropertyToID("_amount");
        private const float ChangeTime = 0.6f;
        [SerializeField] private int id;
        public static readonly Dictionary<int ,SpawnPoint> ActiveSpawnPoints = new();
    
        private void Awake()
        {
            _myMaterial = GetComponent<MeshRenderer>().material;
            ActiveSpawnPoints.Add(id,this);
        }

        private void OnDestroy()
        {
            ActiveSpawnPoints.Remove(id);
        }


        //Can only collide with owner ball
        private void OnTriggerEnter(Collider other)
        {
            //If there is no spawn point
            if (!CurrentSpawnPoint)
            {
                SetActive();
                return;
            }

            if (CurrentSpawnPoint == this) return;

            CurrentSpawnPoint.ChangeColor();
            SetActive();
        }

        void SetActive()
        {
            StopAllCoroutines();
            ChangeColor();
            CurrentSpawnPoint = this;
        }

        private async void ChangeColor()
        {
            float cT = 0;
            float start = this == CurrentSpawnPoint?1:0;
            float end = 1-start;
        
            while (cT < ChangeTime)
            {
                cT += Time.deltaTime;
                _myMaterial.SetFloat(AmountID, Mathf.Lerp(start, end, cT/ChangeTime));
                await UniTask.Yield();
            }
        }
    

    }
}
