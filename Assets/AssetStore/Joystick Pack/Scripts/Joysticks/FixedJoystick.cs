using System.Collections;
using System.Collections.Generic;
using AssetStore.Joystick_Pack.Scripts.Base;
using UnityEngine;

public class FixedJoystick : Joystick
{
    public override void HandleInput(float magnitude, Vector2 normalised)
    {
        base.HandleInput(magnitude, normalised);
    }
}