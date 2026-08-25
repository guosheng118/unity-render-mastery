Shader "RenderingLab/DebugOverlay"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Overlay"
            ZTest Always ZWrite Off Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _LabDebugMode;

            half4 Frag(Varyings i) : SV_Target
            {
                // Tiny legend strip at the top; the actual debug colors come from StylizedLit.
                float2 uv = i.texcoord;
                if (uv.y < 0.96) return 0;
                half3 c = 0;
                if (_LabDebugMode < 1.5) c = half3(0.8,0.8,0.8);
                else if (_LabDebugMode < 2.5) c = half3(0.9,0.7,0.2);
                else if (_LabDebugMode < 3.5) c = half3(0.2,0.2,0.2);
                else if (_LabDebugMode < 4.5) c = half3(0.3,0.6,1);
                else c = half3(0.5,0.9,1);
                return half4(c, 0.85);
            }
            ENDHLSL
        }
    }
}
