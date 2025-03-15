using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Hover;

namespace Gameplay.UI
{
    public class AbilityHandler : MonoBehaviour
    {
        //These should be serialized to prevent issues with setting...
        [SerializeField] private Canvas root;
        [SerializeField] private UIHoldReleaseButton button;
        [SerializeField] private Image fillImg;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI remainingNum;
    
        private BallPlayer _ball;
        private AbilityStats _boundAbility;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        
        private int _capacity;
        private bool _isOnCooldown;
        private bool _wantsToUseAbility;
        
        public void SetAbility(AbilityStats ability, BallPlayer owner)
        {
            bool x = ability;
            root.enabled = x;

            if (!x)
            {
                return;
            }
            
            Debug.Log($"Binding Ability {ability.name}", gameObject);


            _boundAbility = ability;
            _ball = owner;
            
            icon.sprite = ability.Icon;
            _capacity = ability.Capacity;
            
            UpdateCapacity();
                
            button.onBeginHold.RemoveAllListeners();
            button.onEndHold.RemoveAllListeners();
            button.onBeginHold.AddListener(StartUsingAbility);
            button.onEndHold.AddListener(StopUsingAbility);
        }

        private void UpdateCapacity()
        {
            if (_capacity == 0)
            {
                button.Interactable = false;
                root.enabled = false;
                //Debug.Log($"Ability is out of charges! {_boundAbility.name}", gameObject);
            }
            else
            {
                button.Interactable = true;
            }
            if(_capacity <= 0) remainingNum.gameObject.SetActive(false);
            else remainingNum.text = _capacity.ToString();
        }
    
        private async UniTask AbilityCooldown(float dur)
        {
            UpdateCapacity();
            _isOnCooldown = true;
            button.Interactable = false;
            
            float curTime = 0;
            
            while (curTime < dur)
            {
                curTime += Time.deltaTime;
                fillImg.fillAmount = curTime / dur;
                await UniTask.Yield();
            }
            fillImg.fillAmount = 1;
            _isOnCooldown = false;
            UpdateCapacity();
        }

        private async UniTask AbilityExecutionLoop(CancellationToken token)
        {
            while (_wantsToUseAbility)
            {
                //Wait until we can use the ability
                await UniTask.WaitUntil(CheckAbilityReady, PlayerLoopTiming.Update, token);
            
                if (_cancellationTokenSource.IsCancellationRequested || !_wantsToUseAbility) return;
                
                _capacity -= 1;
                _boundAbility.Ability.ExecuteAbility(_ball);
                
                await AbilityCooldown(_boundAbility.Cooldown);
            }
        }
        
        private bool CheckAbilityReady()
        {
            return !_isOnCooldown && _capacity != 0 && _boundAbility.Ability.CanUseAbility(_ball);
        }


        public void StartUsingAbility()
        {
            _wantsToUseAbility = true;
            _ = AbilityExecutionLoop(_cancellationTokenSource.Token);
        }

        public void StopUsingAbility()
        {
            _wantsToUseAbility = false;
        }


        public void SetUsingState(bool state)
        {
            if(state) StartUsingAbility();
            else StopUsingAbility();
        }
    }
}
