using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UIHelper : MonoBehaviour
{
    private Canvas canvas;
    [SerializeField] private TextMeshProUGUI lobbyCode;
    [SerializeField] private TMP_InputField lobbyInput;

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


}
