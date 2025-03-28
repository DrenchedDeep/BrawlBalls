using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Loading
{
    [RequireComponent(typeof(FadeAllBelow), typeof(Canvas)), DefaultExecutionOrder(-200)]
    public class LoadingHelper : MonoBehaviour
    {
        [SerializeField] private Slider progressBar; 
        [SerializeField] private TextMeshProUGUI infoText;
        
        public static LoadingHelper Instance { get; set; }
        private FadeAllBelow _fadeAllBelow;
        private Canvas _canvas;

        private void OnEnable()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            _fadeAllBelow = GetComponent<FadeAllBelow>();
            _canvas = GetComponent<Canvas>();
            
            _canvas.enabled = false;
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
 
            _fadeAllBelow.onFaded.AddListener(() =>_canvas.enabled = false);
            _fadeAllBelow.onUnFaded.AddListener(() => _canvas.enabled = true);

            progressBar.gameObject.SetActive(false);
            infoText.enabled = false;
        }

        public void Activate() => _fadeAllBelow.SetState(true);
        public void Deactivate() => _fadeAllBelow.SetState(false);

        public void SetProgress(float value, bool alwaysShow = false)
        {
            progressBar.gameObject.SetActive(alwaysShow || value is > 0 and < 1);
            progressBar.value = value;
        }

        public async void SetText(string text, float duration = 3)
        {
            infoText.enabled = true;
            infoText.text = text;

            await UniTask.Delay((int)(1000 * duration));


            infoText.enabled = false;
        }


    }
}
