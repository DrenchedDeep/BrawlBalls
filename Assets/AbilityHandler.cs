using System.Collections;
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
    [SerializeField] private TextMeshProUGUI failTex;
    
    private int capacity;
    private bool useAbility = true;
    [SerializeField] private AnimationCurve failTexDisplayAlpha;
    private Coroutine spamPrevention;
    
    
    //Discard temporary information and prevent leaks.
    public void SetAbility(AbilityStats ability, Ball owner, Weapon weapon)
    {
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
        string failText = "";
        button.onClick.AddListener(() =>
        {
            if (useAbility && ability.MyAbility.ActivateAbility(owner, weapon, out failText))
            {
                capacity -= 1;
                StartCoroutine(AbilityCooldown(ability.Cooldown));
            }
            else
            {
                failTex.text = "Can't Use Ability<br>";
                if (!useAbility) failTex.text += "On cooldown";
                else failTex.text += failText;
                //If the button is being spammed, restart it.
                if(spamPrevention!=null) StopCoroutine(spamPrevention);
                spamPrevention = StartCoroutine(ShowFailText());
            }
        });
    }

    private IEnumerator ShowFailText()
    {
        float curTime = 0;
        Color c = failTex.color;
        while (curTime < 1)
        {
            curTime += Time.deltaTime;
            c.a = failTexDisplayAlpha.Evaluate(curTime);
            failTex.color = c;
            yield return null;
        }
        c.a = 0;
        failTex.color = c;
        spamPrevention = null;

    }
    
    
    private IEnumerator AbilityCooldown(float dur)
    {
        if(capacity == 1) remainingNum.gameObject.SetActive(false);
        else if (capacity == 0) //weird format but cope
        {
            gameObject.SetActive(false);
            yield break;
        }
        else remainingNum.text = capacity.ToString();
        useAbility = false;
        float curTime = 0;
        while (curTime < dur)
        {
            curTime += Time.deltaTime;
            fillImg.fillAmount = curTime / dur;
            yield return null;
        }

        fillImg.fillAmount = 1;
        useAbility = capacity > 0;
    }

    
}
