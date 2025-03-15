using Gameplay;
using Gameplay.Balls;
using UnityEngine;

/*
 * ON COIN COLLECTED, GIVE AN ADDITIONAL COIN
 */
public class GoldBall : Ball
{
    public static int AwardMultiplier { get; } = 2;
} 
