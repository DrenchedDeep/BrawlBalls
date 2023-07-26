Shader "Unlit/CavnasMask"
{
    //https://alastaira.wordpress.com/2014/12/27/using-the-stencil-buffer-in-unity-free/
    SubShader
    {
        Tags { "Queue" = "Geometry-100" }  // Write to the stencil buffer before drawing any geometry to the screen
        ColorMask 0 // Don't write to any colour channels
        ZWrite off // Don't write to the Depth buffer
        Pass
        {
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }
        }
    }
}
