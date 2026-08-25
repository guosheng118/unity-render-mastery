Shader "RenderingLab/WetGround"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (0.07,0.08,0.1,1)
        _Smoothness("Smoothness", Range(0,1)) = 0.92
        _Metallic("Metallic", Range(0,1)) = 0.15
        _ReflectionStrength("Planar Strength", Range(0,2)) = 0.85
        _FresnelPower("Fresnel", Range(0.2,8)) = 3
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Smoothness;
                half _Metallic;
                half _ReflectionStrength;
                half _FresnelPower;
            CBUFFER_END

            TEXTURE2D(_PlanarReflectionTexture);
            SAMPLER(sampler_PlanarReflectionTexture);
            float _LabPlanarEnabled;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            Varyings Vert(Attributes i)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(i.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                o.screenPos = ComputeScreenPos(p.positionCS);
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float3 N = normalize(i.normalWS);
                float3 V = normalize(GetWorldSpaceViewDir(i.positionWS));
                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                Light mainLight = GetMainLight(shadowCoord, i.positionWS, 1);
                half ndotl = saturate(dot(N, mainLight.direction));
                half3 diffuse = _BaseColor.rgb * (0.15 + ndotl * mainLight.color * mainLight.shadowAttenuation);

                float2 uv = i.screenPos.xy / i.screenPos.w;
                uv.x = 1 - uv.x; // mirrored camera
                half3 planar = SAMPLE_TEXTURE2D(_PlanarReflectionTexture, sampler_PlanarReflectionTexture, uv).rgb;
                half fresnel = pow(1.0h - saturate(dot(N, V)), _FresnelPower);
                half planarMix = fresnel * _ReflectionStrength * _LabPlanarEnabled * _Smoothness;

                half3 spec = pow(saturate(dot(N, normalize(V + mainLight.direction))), lerp(8, 64, _Smoothness)) * mainLight.color * _Metallic;
                half3 color = lerp(diffuse, planar, planarMix) + spec;
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
