using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    //Includes both ground (default) and bouncey layers.
    public static int GroundLayers { get; private set; }
    public static int ImmortalLayer { get; private set; }
    public static int PlayerLayers { get; private set; }


    // Start is called before the first frame update
    void Awake()
    {
        GroundLayers = (1 << LayerMask.NameToLayer("Default"));
        PlayerLayers = (1 << LayerMask.NameToLayer("Enemy")) + (1 << LayerMask.NameToLayer("Ball"));
        ImmortalLayer = (LayerMask.NameToLayer("Immortal"));
        Application.targetFrameRate = -1; // native default... (BIND IN SETTINGS LATER)
    }

    public static Ball[] ConstructBalls()
    {

        Ball[] balls = new Ball[3];
        for (int i = 0; i < 3; ++i)
        {
            Ball b = Instantiate(PlayerBallInfo.GetBall(i), Level.Instance.PodiumPoints[i].position, Level.Instance.PodiumPoints[i].rotation);
            Weapon w = Instantiate(PlayerBallInfo.GetWeapon(i));
            b.SetAbility(PlayerBallInfo.GetAbility(i));
            b.SetWeapon(w);
            balls[i] = b;
        }

        return balls;

    }

}
