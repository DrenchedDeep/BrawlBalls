using System;
using Gameplay;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEnter : MonoBehaviour
{
    [SerializeField] private UnityEvent<BallPlayer> onEnter;
    [SerializeField] private UnityEvent<BallPlayer> onExit;

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BallPlayer ballPlayer))
        {
            onEnter.Invoke(ballPlayer);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out BallPlayer ballPlayer))
        {
            onExit.Invoke(ballPlayer);
        }
    }
}
