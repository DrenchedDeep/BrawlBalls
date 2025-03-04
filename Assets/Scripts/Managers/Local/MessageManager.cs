using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Managers.Local
{
    public class MessageManager : MonoBehaviour
    {
        
        [Header("Screen messages")]
        [SerializeField] private TextMeshProUGUI tmp;
        [SerializeField] private AnimationCurve animCurve;
        
        public static MessageManager Instance  { get; private set; }
    
    
        // Start is called before the first frame update
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            print("Message Handler is Awake!");
            Instance = this;
        }
    
        public async UniTask HandleScreenMessage(string words, float duration)
        {
            float ct = 0;
            Color c = tmp.color;
            tmp.text = words;
            c.a = 1;
            while (ct < duration)
            {
                ct += Time.deltaTime;
                c.a = animCurve.Evaluate(ct / duration);
                tmp.color = c;
                await UniTask.Yield();
            }
            tmp.color = c;
        }

    }
}
