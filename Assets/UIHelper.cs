using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHelper : MonoBehaviour
{
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    public void ChangeCanvasPosition(float newVal)
    {
        canvas.planeDistance = newVal;
    }
    
}
