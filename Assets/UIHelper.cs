using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHelper : MonoBehaviour
{
    private Canvas canvas;
    [SerializeField] private TextMeshProUGUI lobbyCode;
    [SerializeField] private TMP_InputField lobbyInput;
    [SerializeField] private Image background;
    [SerializeField] private Camera cam;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        lobbyInput.onSelect.AddListener((x) =>
        {
            string s = UniClipboard.GetText();
            if (string.IsNullOrEmpty(x) && s.Length == 6 && !s.Any(char.IsLower))
            {
                //Try auto joining lobby 
                lobbyInput.text = x;
            }
        });
        lobbyInput.onValidateInput += delegate(string s, int i, char c)
        {
            if (i > 6) return '\0';
            return char.IsLetter(c) ? char.ToUpper(c) : '\0'; 
        };
    }

    public void ChangeCanvasPosition(float newVal)
    {
        canvas.planeDistance = newVal;
    }

    public void CopyLobbyCode()
    {
        //Only allow if either is host, or 
        UniClipboard.SetText(lobbyCode.text);
    }

    public void OpenDiscord()
    {
        Application.OpenURL("https://discord.com/invite/duvv2wZNpf");
    }

    public void TransitionBg(bool x)
    {
        if(trans != null) StopCoroutine(trans);
        trans = StartCoroutine(Transition(x));
    }

    private const float TransitionDuration = 0.4f;
    private static readonly Color end = new (1, 0.3f, 0.3f);
    private Coroutine trans;
    private const float FOVA = 60;
    private const float FOVB = 100;
    private IEnumerator Transition(bool x)
    {
        float curTime = 0;
        Color A;
        Color B;
        float fovA;
        float fovB;
        if (x)
        {
            A = Color.white;
            B = end;
            fovA = FOVA;
            fovB = FOVB;
        }
        else
        {
            A = end;
            B = Color.white;
            fovB = FOVA;
            fovA = FOVB;
        }

        while (curTime < TransitionDuration)
        {
            curTime += Time.deltaTime;
            float perc = Mathf.Pow(curTime / TransitionDuration, 2);
            background.color = Color.Lerp(A, B, perc);
            cam.fieldOfView = Mathf.Lerp(fovA, fovB, perc);
            yield return null;
        }

        trans = null;
    }




}
