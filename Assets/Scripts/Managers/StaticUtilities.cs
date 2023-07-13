using System.Collections.Generic;
using UnityEngine;

public static class StaticUtilities
{
    
    public static readonly int RandomTexID = Shader.PropertyToID("_Tex");
    public static readonly int ColorID = Shader.PropertyToID("_Color");
    public static readonly int RandomOffsetID = Shader.PropertyToID("_Offset");
    public static readonly int SpeedID = Shader.PropertyToID("_Speed");
    public static readonly int NoiseSpeedID = Shader.PropertyToID("_NoiseSpeed");
    public static readonly int SecondaryColorID = Shader.PropertyToID("_SecondaryColor");
    
    //Particle
    public static readonly int PositionID = Shader.PropertyToID("Position");
    public static readonly int ActivateID = Shader.PropertyToID("Activate");
    public static readonly int EndID = Shader.PropertyToID("End");
    public static readonly int DelayID = Shader.PropertyToID("initDelay");

    
    


}
