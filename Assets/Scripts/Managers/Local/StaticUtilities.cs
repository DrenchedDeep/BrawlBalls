using UnityEngine;

namespace Managers.Local
{
    public static class StaticUtilities
    {
    
        #region ShaderMaterials

        public static readonly int RandomTexID = Shader.PropertyToID("_Tex");
        public static readonly int ColorID = Shader.PropertyToID("_Color");
        public static readonly int EmissiveID = Shader.PropertyToID("_Emissive");
        public static readonly int RandomOffsetID = Shader.PropertyToID("_Offset");
        public static readonly int SpeedID = Shader.PropertyToID("_Speed");
        public static readonly int NoiseSpeedID = Shader.PropertyToID("_NoiseSpeed");
        public static readonly int SecondaryColorID = Shader.PropertyToID("_SecondaryColor");
        
        #endregion
        
        #region Particle
      
        //TODO May be old, this is generally used for GPU particles
        public static readonly int PositionID = Shader.PropertyToID("Position");
        public static readonly int ActivateID = Shader.PropertyToID("Activate");
        public static readonly int EndID = Shader.PropertyToID("End");
        public static readonly int DelayID = Shader.PropertyToID("initDelay");
        #endregion

        #region Layers
        
        public static readonly int LocalBallLayerLiteral = LayerMask.NameToLayer("Ball");
        public static readonly int ImmortalLayerLiteral = LayerMask.NameToLayer("Immortal");
        public static readonly int EnemyLayerLiteral = LayerMask.NameToLayer("Enemy");
        
        public static readonly int DefaultLayer = 1 << LayerMask.NameToLayer("Default");
        public static readonly int WaterLayer = 1 << LayerMask.NameToLayer("Water");
        public static readonly int InteractableLayer = 1 << LayerMask.NameToLayer("Interactable");
        public static readonly int BouncyLayer = 1 << LayerMask.NameToLayer("Bouncy");
        public static readonly int PodiumLayer = 1 << LayerMask.NameToLayer("Podium");
        public static readonly int UILayer = 1 << LayerMask.NameToLayer("UI");
        
        public static readonly int ImmortalLayer = 1 << ImmortalLayerLiteral;
        public static readonly int EnemyLayer= 1 << EnemyLayerLiteral;
        public static readonly int LocalBallLayer = 1 << LocalBallLayerLiteral;


        //Both Default (Ground layer) and Bouncey layer
        public static readonly int GroundLayers = DefaultLayer | BouncyLayer;
        
        //Ball (local), Enemy (other)
        public static readonly int PlayerLayers = LocalBallLayer | EnemyLayer;
        public static readonly int PodiumBlockers = PodiumLayer | UILayer;

        #endregion

    }
}
