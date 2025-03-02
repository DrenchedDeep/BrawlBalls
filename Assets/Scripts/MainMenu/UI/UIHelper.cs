using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.UI
{
    public class UIHelper : MonoBehaviour
    {
        private Canvas _canvas;
        [SerializeField] private TextMeshProUGUI lobbyCode;
        [SerializeField] private TMP_InputField lobbyInput;
        [SerializeField] private Image background;
        [SerializeField] private Camera cam;

        [Header("Transition")]
        [SerializeField] private float transitionDuration = 0.4f;
        [SerializeField] private float fova = 60;
        [SerializeField] private float fovb = 100;
        [SerializeField] private Color end = new (1, 0.3f, 0.3f);
        private UniTask? _task;
        
        private void Start()
        {
            _canvas = GetComponent<Canvas>();
            lobbyInput.onSelect.AddListener((x) =>
            {
                string s = UniClipboard.GetText();
                if (string.IsNullOrEmpty(x) && s.Length == 6 && !s.Any(char.IsLower))
                {
                    //Try auto joining lobby 
                    lobbyInput.text = x;
                }
            });
            lobbyInput.onValidateInput += delegate(string s, int i, char c)
            {
                if (i > 6) return '\0';
                return char.IsLetter(c) ? char.ToUpper(c) : '\0'; 
            };
        }

        public void ChangeCanvasPosition(float newVal)
        {
            _canvas.planeDistance = newVal;
        }

        public void CopyLobbyCode()
        {
            //Only allow if either is host, or 
            UniClipboard.SetText(lobbyCode.text);
        }

        public void OpenDiscord()
        {
            Application.OpenURL("https://discord.gg/kw5ysTbtHJ");
        }

        public void TransitionBg(bool x)
        {
            //Only assign the task if it does not currently exist
            _task ??= Transition(x);
            Debug.LogWarning("What does this do again?", gameObject);
        }


        private async UniTask Transition(bool x)
        {
            float curTime = 0;
            Color a;
            Color b;
            float fovA;
            float fovB;
            if (x)
            {
                a = Color.white;
                b = end;
                fovA = fova;
                fovB = fovb;
            }
            else
            {
                a = end;
                b = Color.white;
                fovB = fova;
                fovA = fovb;
            }

            while (curTime < transitionDuration)
            {
                curTime += Time.deltaTime;
                float perc = Mathf.Pow(curTime / transitionDuration, 2);
                background.color = Color.Lerp(a, b, perc);
                cam.fieldOfView = Mathf.Lerp(fovA, fovB, perc);
                await UniTask.Yield();
            }

            _task = null;
        }




    }
}
