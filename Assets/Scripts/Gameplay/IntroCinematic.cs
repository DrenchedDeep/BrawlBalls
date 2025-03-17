using System;
using System.Collections;
using Managers.Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class IntroCinematic : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private TimelineAsset timeline;
    public void OnEnable()
    {
        director.Play();
        StartCoroutine(Cutscene());
    }


    private IEnumerator Cutscene()
    {
        yield return new WaitForSeconds((float)timeline.duration);
        NetworkGameManager.Instance.ClientFinishedIntroCinematic_ServerRpc();
    }
    
}
