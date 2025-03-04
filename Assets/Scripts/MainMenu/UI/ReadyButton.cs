using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.UI
{
    public class ReadyButton : MonoBehaviour
    {

        [SerializeField] private Button readyButton;
        [SerializeField] private Image readyBackground;
        private const float ReadyDelay = 1.5f;
        public void StartDelay()
        {
            _ = PauseButton();
        }

        private async UniTask PauseButton()
        {
            readyButton.interactable = false;
            float curTime = 0;
            while (curTime < ReadyDelay)
            {
                curTime += Time.deltaTime;
                readyBackground.fillAmount = curTime / ReadyDelay;
                await UniTask.Yield();
            }

            readyButton.interactable = true;
        }

    }
}
