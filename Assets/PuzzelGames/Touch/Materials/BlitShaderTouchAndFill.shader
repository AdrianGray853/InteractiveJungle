Shader "Custom/BlitTouchAndFill"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _BrushPos("Brush Position", Vector) = (0,0,0,0) // X, Y
        _BrushSettings("Brush Settings", Vector) = (0,0,0,0) // Scale, Hardness, Rotation
        _BrushColor("Brush Color", Color) = (1,1,1,1)
        _Boundings("Boundings", Vector) = (0,0,1,1) // minX, minY, maxX, maxY
    }
        SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100

        ZWrite Off
        Cull Off

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Interpolation.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _BrushPos;
            float4 _BrushSettings;
            fixed4 _BrushColor;
            float4 _Boundings;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
//                float2 dist = i.uv - _BrushStart.xy;

                fixed4 endColor = _BrushColor;
                endColor.a = 0;
                //float interp = saturate(length(dist) * _BrushStart.z);
                float dist = distance(i.uv * _MainTex_TexelSize.zw, _BrushPos.xy);
                float interp = saturate(dist * _BrushSettings.x); 
                interp = remap(_BrushSettings.y, 1.0, 0.0, 1.0, interp);

                fixed4 col = lerp(_BrushColor, endColor, interp);
                if (i.uv.x < _Boundings.x || i.uv.x > _Boundings.z
                    || i.uv.y < _Boundings.y || i.uv.y > _Boundings.w)
                {
                    col.a = 0;
                }

                col = tex2D(_MainTex, i.uv) * col;

                return col;
            }
            ENDCG
        }
    }
}
