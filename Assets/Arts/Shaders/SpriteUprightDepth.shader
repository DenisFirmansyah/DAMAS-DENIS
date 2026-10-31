Shader "DamasDenis/SpriteUprightDepth"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [PerRendererData] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [PerRendererData] _Flip ("Flip", Vector) = (1,1,1,1)
        _DepthBias ("Depth Bias (world units)", Float) = 0.05
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }

        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;
            float4 _RendererColor;
            float4 _Flip;
            float  _DepthBias;

            // Diisi per karakter lewat MaterialPropertyBlock (dari Billboard.cs)
            float3 _PivotWS;
            float3 _PlaneNormalWS;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 posOS = IN.positionOS;
                posOS.xy *= _Flip.xy;

                float3 P = TransformObjectToWorld(posOS);   // posisi asli (menentukan posisi layar)
                float3 C = _WorldSpaceCameraPos;
                float3 D = P - C;

                // Perpotongan ray kamera -> vertex dengan bidang vertikal di pivot karakter
                float denom = min(dot(D, _PlaneNormalWS), -1e-4);
                float t = dot(_PivotWS - C, _PlaneNormalWS) / denom;
                t = clamp(t, 0.1, 10.0);
                float3 Q = C + D * t + _PlaneNormalWS * _DepthBias;

                float4 clipP = TransformWorldToHClip(P);
                float4 clipQ = TransformWorldToHClip(Q);

                OUT.positionCS = clipP;
                // Posisi layar dari P, depth dari Q
                OUT.positionCS.z = (clipQ.z / clipQ.w) * clipP.w;

                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color * _RendererColor;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;
                clip(c.a - 0.001);
                return c;
            }
            ENDHLSL
        }
    }
}