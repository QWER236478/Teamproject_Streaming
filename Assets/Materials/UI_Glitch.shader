Shader "UI/Glitch"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _Intensity ("Glitch Intensity", Range(0,1)) = 0.35
        _Speed ("Speed", Range(0,10)) = 2.0
        _BlockSize ("Block Rows", Range(4,200)) = 60
        _Split ("RGB Split", Range(0,5)) = 1.2

        // UI 표준 프로퍼티 (마스크/배칭용)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI-Glitch"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 uv            : TEXCOORD0;
                float4 worldPosition : TEXCOORD1; // UI 마스킹
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float4 _ClipRect;

            float _Intensity;
            float _Speed;
            float _BlockSize;
            float _Split;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            // 간단 난수
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // RectMask2D/Mask 지원
                #ifdef UNITY_UI_CLIP_RECT
                    fixed alpha = UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                    if (alpha <= 0) discard;
                #endif

                float2 uv = i.uv;

                // 가로 줄 단위로 X축을 랜덤 쉬프트
                float row = floor(uv.y * _BlockSize);
                float n = hash21(float2(row, floor(_Time.y * _Speed * 60)));
                float shift = (n - 0.5) * 0.06 * _Intensity; // 좌우 ±
                uv.x += shift;

                // RGB Split (약한 색수차)
                float2 off = float2(_Split * 0.002 * _Intensity, 0);
                fixed r = tex2D(_MainTex, uv + off).r;
                fixed g = tex2D(_MainTex, uv).g;
                fixed b = tex2D(_MainTex, uv - off).b;
                fixed a = tex2D(_MainTex, uv).a;

                fixed4 col = fixed4(r,g,b,a) * i.color;

                // 얇은 스캔라인/깜빡임
                float sl = sin(uv.y * 700 + _Time.y * 25) * 0.03 * _Intensity;
                col.rgb -= sl;

                #ifdef UNITY_UI_ALPHACLIP
                    clip(col.a - 0.001);
                #endif
                return col;
            }
            ENDCG
        }
    }
}