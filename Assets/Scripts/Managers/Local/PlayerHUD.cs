using System;
using Cysharp.Threading.Tasks;
using Managers.Network;
using TMPro;
using UnityEngine;

/// <summary>
/// EVERY CLIENT HAS THEIR OWN PLAYER HUD.... DONT ADD A SINGLETON
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    private static readonly int Show = Animator.StringToHash("Show");
    private static readonly int Hide = Animator.StringToHash("Hide");

    [Header("Elimination UI Settings")]
    
    [SerializeField] private Animator eliminationAnimator;
    [SerializeField] private TextMeshProUGUI eliminationPlayerText;
    
    [Header("Starting Match Timer")]

    [SerializeField] private TextMeshProUGUI startingMatchTimerText;
    [SerializeField] private Animator startingMatchTimerAnimator;

    [Header("Game Match Timer")]
    [SerializeField] private TextMeshProUGUI matchTimerText;
    

    
    private void OnEnable()
    {
        NetworkGameManager.Instance.OnGameCountdownDecremented += OnGameStartCountdownDecremented;
        NetworkGameManager.Instance.OnCurrentTimeChanged += OnGameCurrentTimeChanged;
    }

    private void OnDisable()
    {
        NetworkGameManager.Instance.OnGameCountdownDecremented -= OnGameStartCountdownDecremented;
        NetworkGameManager.Instance.OnCurrentTimeChanged -= OnGameCurrentTimeChanged;

    }

    private void OnGameStartCountdownDecremented(int value)
    {
        string text = value <= 0 ? "GO!" : value.ToString();
        Debug.Log("I HATE MY LIFE");
        startingMatchTimerText.text = text;
    }

    private void OnGameCurrentTimeChanged(float value)
    {
        //only update this part if the gamestate == InGame
        if (NetworkGameManager.Instance.GameState.Value == GameState.InGame)
        {
            var ts = TimeSpan.FromSeconds(NetworkGameManager.Instance.GetRemainingTime);
            matchTimerText.text = ts.ToString("mm\\:ss");
        }
    }


    /// <summary>
    /// WRAPPER FOR THE UNI-TASK -- this was originally called form the NetworkGameManager -- will need to be rewritten cuz of no singletons allowed :P -- check GAMEUI to see how it was done originally
    /// (YES I KNOW THAT CLASS IS A SIN)
    /// </summary>
    /// <param name="victimName"></param>
    public void OnKilledPlayer(string victimName) => _ = OnKilledPlayerTask(victimName);

    private async UniTask OnKilledPlayerTask(string victimName)
    {
        eliminationAnimator.gameObject.SetActive(true);
        eliminationAnimator.SetTrigger(Show);
        eliminationPlayerText.text = victimName;

        await UniTask.WaitForSeconds(1f);
        
        eliminationAnimator.SetTrigger(Hide);

        await UniTask.WaitForSeconds(0.5f);
        
        eliminationAnimator.gameObject.SetActive(false);
    }

}
