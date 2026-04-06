Shader "Custom/StoveBlueFire"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.03, 0.18, 1.0, 0.75)
        _BottomColor ("Bottom Color", Color) = (0.45, 0.9, 1.0, 0.4)
        _WorldYMin ("World Y Min", Float) = 0.0
        _WorldYMax ("World Y Max", Float) = 0.4
        _Opacity ("Opacity", Range(0, 1)) = 0.75
        _AlphaPower ("Alpha Power", Range(0.2, 4.0)) = 1.5
        _SoftEdge ("Soft Edge", Range(0.0, 0.5)) = 0.12
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _BottomColor;
                float _WorldYMin;
                float _WorldYMax;
                half _Opacity;
                half _AlphaPower;
                half _SoftEdge;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float heightRange = max(_WorldYMax - _WorldYMin, 0.0001);
                half gradient = saturate((input.worldPos.y - _WorldYMin) / heightRange);

                // Top is deeper blue, bottom is lighter cyan-blue.
                half4 col = lerp(_BottomColor, _TopColor, gradient);

                // Slightly reduce alpha near bottom and near UV edges for softer flame cards.
                half edgeMask = smoothstep(_SoftEdge, 0.5h, abs(input.uv.x - 0.5h));
                half alphaGradient = pow(gradient, _AlphaPower);
                col.a *= _Opacity * alphaGradient * edgeMask;

                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
