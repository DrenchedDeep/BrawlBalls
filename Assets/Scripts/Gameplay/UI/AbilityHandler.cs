using Cysharp.Threading.Tasks;
using Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class AbilityHandler : MonoBehaviour
    {
        //These should be serialized to prevent issues with setting...
        [SerializeField] private Button button;
        [SerializeField] private Image fillImg;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI remainingNum;
    
        private int _capacity;
        private Ability _ability;
        private BallPlayer _ball;
        private bool _isUpdating = true;
    
        //Discard temporary information and prevent leaks.
        public void SetAbility(AbilityStats ability, BallPlayer owner)
        {
            print("Active!");
            enabled = true;
            _ball = owner;
            _ability = ability.Ability;

            button.onClick.RemoveAllListeners();
            StopAllCoroutines();
            _ = AbilityCooldown(0); //??? What was going on here?

        
            button.interactable = true;
        
            if (!ability)
            {
                gameObject.SetActive(false);
                return;
            }

        
            gameObject.SetActive(true);

            _capacity = ability.Capacity;
            if(_capacity <= 1) remainingNum.gameObject.SetActive(false);
            else remainingNum.text = _capacity.ToString();
        
            icon.sprite = ability.Icon;

        

            button.onClick.AddListener(() => TryUseAbility(ability, owner));
        }

        public void TryUseAbility(AbilityStats ability, BallPlayer owner)
        {
            if (ability.Ability.CanUseAbility(owner) && ability.Ability.ActivateAbility(owner))
            {
                _capacity -= 1;
                _ = AbilityCooldown(ability.Cooldown);
                Debug.Log($"Activating {ability.name} Ability!");
            }
        }

        private void Start()
        {
            enabled = false;
        }

        private void Update()
        {
            if (!_isUpdating) return;
            button.interactable = _ability.CanUseAbility(_ball);
        }


        private async UniTask AbilityCooldown(float dur)
        {
            button.interactable = false;
            _isUpdating = false;
            if(_capacity == 1) remainingNum.gameObject.SetActive(false);
            else if (_capacity == 0) //weird format but cope
            {
                gameObject.SetActive(false);
                return;
            }
            else remainingNum.text = _capacity.ToString();
            float curTime = 0;
            while (curTime < dur)
            {
                curTime += Time.deltaTime;
                fillImg.fillAmount = curTime / dur;
                await UniTask.Yield();
            }

            button.interactable = true;
            fillImg.fillAmount = 1;
            _isUpdating = true;
        }

    
    }
}
