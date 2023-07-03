using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class LaserEffectHandler : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private VisualEffect myEffect;
    [SerializeField] private AnimationCurve initBeam;
    

    //This needs to be a networked function... Client RPC?
    public void Begin(float timeToActivate, float width, float distance)
    {
        
        myEffect.SendEvent(ParticleManager.ActivateID);
        myEffect.SetVector3(ParticleManager.PositionID, Vector3.forward * distance);
        lineRenderer.SetPosition(1,Vector3.forward * distance);
        StartCoroutine(HandleSizeChange(timeToActivate, width));
    }

    private IEnumerator HandleSizeChange(float timeToActivate, float width)
    {
        float curTime = 0;

        while (curTime < timeToActivate)
        {
            curTime += Time.deltaTime;
            lineRenderer.startWidth = Mathf.Lerp(0.01f, width, initBeam.Evaluate(curTime / timeToActivate));
            yield return null;
        }
    }


    public void End()
    {
        
    }
}
