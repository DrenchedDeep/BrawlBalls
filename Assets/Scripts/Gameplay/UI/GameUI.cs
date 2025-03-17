using System;
using System.Collections;
using Managers.Network;
using TMPro;
using UnityEngine;

namespace Gameplay.UI
{
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        [Header("Elim UI")]
        [SerializeField] private Animator eliminationAnimator;
        [SerializeField] private TextMeshProUGUI eliminationPlayerText;

        [Header("Game Match Timer")]
        [SerializeField] private TextMeshProUGUI matchTimerText;
        
        [Header("Starting Match Timer")]

        [SerializeField] private TextMeshProUGUI startingMatchTimerText;
        [SerializeField] private Animator startingMatchTimerAnimator;

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
            if (NetworkGameManager.Instance.GameState.Value == GameState.StartingGame)
            {
                int time = (int)NetworkGameManager.Instance.GetStartingMatchTime;
                if (time <= 0.2)
                {
                    startingMatchTimerText.text = "GO!";
                }
                else
                {
                    startingMatchTimerText.text = (time).ToString();
                }
            }
            else if (NetworkGameManager.Instance.GameState.Value == GameState.InGame)
            {
                var ts = TimeSpan.FromSeconds(NetworkGameManager.Instance.GetRemainingTime);
                matchTimerText.text = ts.ToString("mm\\:ss");
            }
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
}
