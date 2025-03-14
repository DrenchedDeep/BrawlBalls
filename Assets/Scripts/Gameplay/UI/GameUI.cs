using System;
using System.Collections;
using Managers.Network;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("Elim UI")]
    [SerializeField] private Animator eliminationAnimator;
    [SerializeField] private TextMeshProUGUI eliminationPlayerText;

    [Header("Match Timer")]
    [SerializeField] private TextMeshProUGUI matchTimerText;

    private static readonly int Show = Animator.StringToHash("Show");
    private static readonly int Hide = Animator.StringToHash("Hide");


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (!NetworkGameManager.Instance.GameStarted.Value)
        {
            matchTimerText.text = "WAITING FOR PLAYERS TO CONNECT...";
            return;
        }
        

        var ts = TimeSpan.FromSeconds(NetworkGameManager.Instance.GetTotalTimePassed);

        matchTimerText.text =  string.Format("{0:00}:{1:00}", (int) ts.TotalMinutes, (int) ts.Seconds);
    }

    public void ShowElimUI(string killedName)
    {
        eliminationAnimator.gameObject.SetActive(true);
        eliminationAnimator.SetTrigger(Show);
        eliminationPlayerText.text = killedName;

        StartCoroutine(HideElimUI());
    }

    private IEnumerator HideElimUI()
    {
        yield return new WaitForSeconds(1);
        eliminationAnimator.SetTrigger(Hide);
        yield return new WaitForSeconds(.25f);
        eliminationAnimator.gameObject.SetActive(false);


        
    }
}
