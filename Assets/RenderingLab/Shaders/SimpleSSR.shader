Shader "RenderingLab/SimpleSSR"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "SSR"
            ZTest Always ZWrite Off Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float _MaxSteps;
            float _Intensity;

            half4 Frag(Varyings i) : SV_Target
            {
                // Educational stub: fade a faint screen-space "wet" based on depth edges.
                // Official URP SSR is expected around Unity 6.7. Prefer probes + planar on 6.3.
                float2 uv = i.texcoord;
                float depth = SampleSceneDepth(uv);
                float depthR = SampleSceneDepth(uv + float2(0.002, 0));
                float edge = saturate(abs(depth - depthR) * 40);
                return half4(0.4, 0.55, 0.7, edge * _Intensity * 0.25);
            }
            ENDHLSL
        }
    }
}
