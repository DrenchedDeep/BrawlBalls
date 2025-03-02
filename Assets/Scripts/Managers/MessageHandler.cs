using System.Collections;
using TMPro;
using UnityEngine;

public class MessageHandler : MonoBehaviour
{
    
    
    
    [Header("Screen messages")]
    [SerializeField] private TextMeshProUGUI tmp;
    private static TextMeshProUGUI _popUpText;
    [SerializeField] private AnimationCurve animCurve;
    private static AnimationCurve _popUpAnimCurve;
    private static MessageHandler _mh;

    
    
    
    [SerializeField] private TextMeshProUGUI[] scores;
    [SerializeField] private TextMeshProUGUI prefab;
    
    [RuntimeInitializeOnLoadMethod]
    private static void RuntimeInit()
    {
        _popUpText = null;
        _popUpAnimCurve = null;
        _mh = null;
    }
    
    
    // Start is called before the first frame update
    private void Start()
    {
        print("NessageHandlerAwake");
        _mh = this;
        _popUpText = tmp;
        _popUpAnimCurve = animCurve;
        
        
    }
    
    public static void SetScreenMessage(string words, float duration)
    {
        _mh.StartCoroutine(HandleScreenMessage(words, duration));
    }

    private static IEnumerator HandleScreenMessage(string words, float duration)
    {
        float ct = 0;
        Color c = _popUpText.color;
        _popUpText.text = words;
        c.a = 1;
        while (ct < duration)
        {
            ct += Time.deltaTime;
            c.a = _popUpAnimCurve.Evaluate(ct / duration);
            _popUpText.color = c;
            yield return null;
        }
        _popUpText.color = c;
    }

}
