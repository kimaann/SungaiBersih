
Shader "Custom/URP/StylizedRiverWater"
{
    Properties
    {
        [Header(Water Colors)]
        _DeepColor ("Deep Water", Color) = (0.035, 0.28, 0.32, 1)
        _MidColor ("Main Water", Color) = (0.08, 0.52, 0.48, 1)
        _HighlightColor ("Wave Highlight", Color) = (0.42, 0.88, 0.76, 1)

        [Header(Flow Settings)]
        _FlowSpeed ("Flow Speed", Range(0, 2)) = 0.45
        _BandScale ("Band Scale", Range(0.01, 1)) = 0.22
        _BandFrequency ("Band Frequency", Range(1, 10)) = 4

        [Header(Wave Settings)]
        _Distortion ("Wave Distortion", Range(0, 1)) = 0.35
        _DetailScale ("Detail Scale", Range(0.1, 5)) = 1.8
        _HighlightStrength ("Highlight Strength", Range(0, 1)) = 0.5

        [Header(Edge Settings)]
        _RiverEdgeWidth ("River Edge Width", Range(0.01, 0.3)) = 0.12
        _EdgeDarkening ("Edge Darkening", Range(0, 1)) = 0.25
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "StylizedWater"

            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Cull Back
            ZWrite On
            Blend One Zero

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)

                float4 _DeepColor;
                float4 _MidColor;
                float4 _HighlightColor;

                float _FlowSpeed;
                float _BandScale;
                float _BandFrequency;

                float _Distortion;
                float _DetailScale;
                float _HighlightStrength;

                float _RiverEdgeWidth;
                float _EdgeDarkening;

            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 worldPos = input.positionWS.xz;
                float time = _Time.y;

                // 1. Main flowing waves
                float crossWave = sin(
                    worldPos.x * 0.8 + time * 0.7
                ) * _Distortion;

                float flowCoordinate =
                    worldPos.y * _BandScale
                    - time * _FlowSpeed
                    + crossWave;

                float mainWave = sin(
                    flowCoordinate * _BandFrequency
                );

                float waveMask = smoothstep(
                    -0.15,
                    0.65,
                    mainWave
                );

                // 2. Small animated water details
                float detailCoordinate =
                    worldPos.y * _DetailScale
                    + worldPos.x * 1.2
                    - time * _FlowSpeed * 2.0;

                detailCoordinate += sin(
                    worldPos.x * 2.5 + time
                ) * 0.7;

                float detailWave = sin(detailCoordinate);

                float highlightMask = smoothstep(
                    0.72,
                    0.93,
                    detailWave * 0.5 + 0.5
                );

                // 3. Cartoon water colors
                float3 waterColor = lerp(
                    _DeepColor.rgb,
                    _MidColor.rgb,
                    0.25 + waveMask * 0.65
                );

                waterColor = lerp(
                    waterColor,
                    _HighlightColor.rgb,
                    highlightMask * _HighlightStrength
                );

                // 4. Darker river edges
                float edgeDistance = min(
                    input.uv.x,
                    1.0 - input.uv.x
                );

                float edgeMask = 1.0 - smoothstep(
                    0.0,
                    _RiverEdgeWidth,
                    edgeDistance
                );

                waterColor = lerp(
                    waterColor,
                    _DeepColor.rgb,
                    edgeMask * _EdgeDarkening
                );

                return half4(waterColor, 1.0);
            }

            ENDHLSL
        }
    }
}
