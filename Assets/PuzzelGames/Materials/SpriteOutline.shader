Shader "Sprite/SpiderOutline"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _BaseColor("Base Color", Color) = (0,0,0,1)
        _ColorOffset("Offset", Color) = (1,1,1,0)
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _Thickness("Thickness", Float) = 1.0
        [MaterialToggle] _GrayscaleImage("Use Grayscale Image", Float) = 0
        [MaterialToggle] _Antialiased("Antialiased", Float) = 0
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile DUMMY PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color : COLOR;
                half2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _BaseColor;
            fixed4 _ColorOffset;
            fixed4 _OutlineColor;
            float _Thickness = 2.0;
            float _GrayscaleImage;
            float _Antialiased;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 SampleTex(float2 UV, v2f IN) {
                fixed4 c = tex2D(_MainTex, UV) * IN.color;
                return c;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleTex(IN.texcoord, IN);
                fixed3 grayscaledColor = lerp(_BaseColor, _Color, c.r);
                c.rgb = lerp(c.rgb * _Color.rgb, grayscaledColor, _GrayscaleImage);
                c.rgb = lerp(c.rgb, _ColorOffset.rgb, _ColorOffset.a);
                if (_Antialiased < 0.5f)
                {
                    c.a = c.a < 0.1 ? 0 : 1.0;
                }
                c.rgb *= c.a;

                if (_Thickness > 0)
                {
                    const float minAlpha = 0.5;
                    #define SQRT2 0.70710678118
                    float thicknessX = _Thickness / _MainTex_TexelSize.z;
                    float thicknessY = _Thickness / _MainTex_TexelSize.w;

                    fixed4 l = SampleTex(IN.texcoord + fixed2(0, +thicknessY), IN);
                    fixed4 r = SampleTex(IN.texcoord + fixed2(0, -thicknessY), IN);
                    fixed4 t = SampleTex(IN.texcoord + fixed2(+thicknessX, 0), IN);
                    fixed4 b = SampleTex(IN.texcoord + fixed2(-thicknessX, 0), IN);
                    fixed4 tr = SampleTex(IN.texcoord + fixed2(+thicknessX * SQRT2, -thicknessY * SQRT2), IN);
                    fixed4 tl = SampleTex(IN.texcoord + fixed2(-thicknessX * SQRT2, +thicknessY * SQRT2), IN);
                    fixed4 bl = SampleTex(IN.texcoord + fixed2(-thicknessX * SQRT2, -thicknessY * SQRT2), IN);
                    fixed4 br = SampleTex(IN.texcoord + fixed2(+thicknessX * SQRT2, +thicknessY * SQRT2), IN);

                    if (c.a > 0 && (l.a < minAlpha || r.a < minAlpha || t.a < minAlpha || b.a < minAlpha ||
                        tl.a < minAlpha || tr.a < minAlpha || br.a < minAlpha || bl.a < minAlpha)) {
                        fixed4 outline = fixed4(_OutlineColor.r, _OutlineColor.g, _OutlineColor.b, c.a);
                        c = lerp(c, outline, _OutlineColor.a);
                    }
                }

                c *= _Color.a;
                return c;
            }
        ENDCG
        }
    }
}