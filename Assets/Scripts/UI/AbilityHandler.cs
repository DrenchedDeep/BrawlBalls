using Cysharp.Threading.Tasks;
using Gameplay.Abilities;
using Gameplay.Balls;
using Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
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
        private NetworkBall _networkBall;
        private Weapon _weapon;
        private bool _isUpdating = true;
    
        //Discard temporary information and prevent leaks.
        public void SetAbility(AbilityStats ability, NetworkBall owner, Weapon weap)
        {
            print("Active!");
            enabled = true;
            _networkBall = owner;
            _ability = ability.MyAbility;
            _weapon = weap;

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
        
            button.onClick.AddListener(() =>
            {
                if (ability.MyAbility.ActivateAbility(owner, _weapon))
                {
                    _capacity -= 1;
                     _ = AbilityCooldown(ability.Cooldown);
                }
            });
        }

        private void Start()
        {
            enabled = false;
        }

        private void Update()
        {
            if (!_isUpdating) return;
            button.interactable = _ability.CanUseAbility(_networkBall, _weapon);
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
