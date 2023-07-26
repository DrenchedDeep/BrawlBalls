Shader "Unlit/ViewMask"
{
    SubShader
    {
        Pass
        {
            Stencil {
              Ref 1
              Comp Equal
            }
        }
    }
}
