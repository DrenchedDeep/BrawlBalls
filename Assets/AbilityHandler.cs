using System;
using System.Collections;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityHandler : MonoBehaviour
{
    //These should be serialized to prevent issues with setting...
    [SerializeField] private Button button;
    [SerializeField] private Image fillImg;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI remainingNum;
    
    private int capacity;
    private Ability ab;
    private Ball ball;
    private Weapon weapon;
    private bool isUpdating = true;
    
    //Discard temporary information and prevent leaks.
    public void SetAbility(AbilityStats ability, Ball owner, Weapon weap)
    {
        print("Active!");
        enabled = true;
        ball = owner;
        ab = ability.MyAbility;
        weapon = weap;

        button.onClick.RemoveAllListeners();
        StopAllCoroutines();
        AbilityCooldown(0); //??? What was going on here?

        
        button.interactable = true;
        
        if (!ability)
        {
            gameObject.SetActive(false);
            return;
        }

        
        gameObject.SetActive(true);

        capacity = ability.Capacity;
        if(capacity <= 1) remainingNum.gameObject.SetActive(false);
        else remainingNum.text = capacity.ToString();
        
        icon.sprite = ability.Icon;
        
        button.onClick.AddListener(() =>
        {
            if (ability.MyAbility.ActivateAbility(owner, weapon))
            {
                capacity -= 1;
                StartCoroutine(AbilityCooldown(ability.Cooldown));
            }
        });
    }

    private void Start()
    {
        enabled = false;
    }

    private void Update()
    {
        if (!isUpdating) return;
        button.interactable = ab.CanUseAbility(ball, weapon);
    }


    private IEnumerator AbilityCooldown(float dur)
    {
        button.interactable = false;
        isUpdating = false;
        if(capacity == 1) remainingNum.gameObject.SetActive(false);
        else if (capacity == 0) //weird format but cope
        {
            gameObject.SetActive(false);
            yield break;
        }
        else remainingNum.text = capacity.ToString();
        float curTime = 0;
        while (curTime < dur)
        {
            curTime += Time.deltaTime;
            fillImg.fillAmount = curTime / dur;
            yield return null;
        }

        button.interactable = true;
        fillImg.fillAmount = 1;
        isUpdating = true;
    }

    
}
