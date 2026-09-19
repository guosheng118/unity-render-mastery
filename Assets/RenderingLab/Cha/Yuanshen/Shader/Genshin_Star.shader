Shader "RenderingLab/Genshin/Star"
{
    properties
    {
        _StarMap("Star Map",2D) = "black" {}
        //_StarColor("Star Color", Color) = (1,1,1,1)
        _TintA("Tint A", Color) = (0.55,0.85,1,1)
        _TintB("Tint B", Color) = (1,0.75,0.95,1)
        _HueSpeed("Hue Speed", Float) = 0.2
        _StarIntensity("Star Intensity", Range(0,5)) = 1
        _RoteSpeed("Rotate Speed", float) = 0.01
    }

    SubShader
    {
        Tags{"RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent"}

        Pass
        {
            Name "Star"
            Tags{"LightMode" = "UniversalForward"}

            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend One One


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _StarMap_ST;
                //float4 _StarColor;
                half4 _TintA, _TintB;
                float _HueSpeed;
                half _StarIntensity;
                float _RoteSpeed;
            CBUFFER_END

            TEXTURE2D(_StarMap); SAMPLER(sampler_StarMap);

            struct Attributus
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributus IN)
            {
                Varyings o = (Varyings)0;
                o.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.uv = TRANSFORM_TEX(IN.uv, _StarMap);
                o.uv.x += _Time.y * _RoteSpeed;
                return o;   
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                half3 star = SAMPLE_TEXTURE2D(_StarMap, sampler_StarMap, IN.uv).rgb;

                float t = 0.5 + 0.5 * sin(_Time.y * _HueSpeed * 2.0 * PI + IN.uv.x * 2.0 * PI);
                half3 tint = lerp(_TintA.rgb, _TintB.rgb, t);

                half3 col = star * tint * _StarIntensity;
                return half4(col, 1);                
            }
            ENDHLSL
        }
    }

}