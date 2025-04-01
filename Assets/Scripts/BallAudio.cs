using System;
using Gameplay;
using UnityEngine;

/// <summary>
/// THIS IS FOR THE BALL AUDIO -- IT CAN BE FOUND ON THE BALLPLAYER PREFAB 
/// </summary>
public class BallAudio : MonoBehaviour
{
    [SerializeField] private BallPlayer ballPlayer;
    
    /// <summary>
    /// DONT CHANGE THIS
    /// </summary>
    private void OnEnable()
    {
        ballPlayer.OnDestroyed += OnBallDied;
        ballPlayer.OnDamaged += OnBallTookDamage;
        ballPlayer.OnHealed += OnBallTookDamage;
    }

    /// <summary>
    /// DONT CHANGE THIS
    /// </summary>
    private void OnDisable()
    {
        ballPlayer.OnDestroyed -= OnBallDied;
        ballPlayer.OnDamaged -= OnBallTookDamage;
        ballPlayer.OnHealed -= OnBallTookDamage;
    }
    
    /// <summary>
    /// SINCE THIS IS ATTACHED TO THE BALL, WHEN IT STARTS IT ACTS AS WHEN THE PLAYER SPAWNS
    /// </summary>
    private void Start()
    {
        OnBallSpawned();
    }

    private void OnBallSpawned()
    {
        
    }

    /// <summary>
    /// DO YOUR FMOD STUFF HERE -- THIS WILL PLAY ON EVERY CLIENT SO EVERYBODY WILL HEAR THE AUDIO... IF U DONT WANT THEM LMK 
    /// </summary>
    private void OnBallTookDamage()
    {
        
    }

    /// <summary>
    /// DO YOUR FMOD STUFF HERE -- THIS WILL PLAY ON EVERY CLIENT SO EVERYBODY WILL HEAR THE AUDIO... IF U DONT WANT THEM LMK 
    /// </summary>
    private void OnBallHealed()
    {
        
    }


    /// <summary>
    /// DO YOUR FMOD STUFF HERE -- THIS WILL PLAY ON EVERY CLIENT SO EVERYBODY WILL HEAR THE AUDIO... IF U DONT WANT THEM LMK 
    /// </summary>
    private void OnBallDied(ulong killer, int childID)
    {
        
    }
}
