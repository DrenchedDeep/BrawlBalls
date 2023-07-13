using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyButton : MonoBehaviour
{

    [SerializeField] private Button readyButton;
    [SerializeField] private Image readyBackground;
    private const float ReadyDelay = 1.5f;
    public void StartDelay()
    {
        StartCoroutine(PauseButton());
    }

    private IEnumerator PauseButton()
    {
        readyButton.interactable = false;
        float curTime = 0;
        while (curTime < ReadyDelay)
        {
            curTime += Time.deltaTime;
            readyBackground.fillAmount = curTime / ReadyDelay;
            yield return null;
        }

        readyButton.interactable = true;
    }

}
