Shader "Custom/StencilFill"
{
    SubShader{
        Tags { "RenderType" = "Transparent" "Queue" = "Geometry-1" }

        ColorMask 0
        ZWrite Off

        Stencil{
            Ref 2
            Comp always
            Pass replace
        }

        Pass{}
    }
}
