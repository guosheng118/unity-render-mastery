Shader "RenderingLab/StylizedLit"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _BumpMap("Normal", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1

        [Header(Ramp Lighting)]
        _RampTex("Ramp (optional)", 2D) = "white" {}
        _RampThreshold("Ramp Threshold", Range(0,1)) = 0.42
        _RampSmooth("Ramp Smooth", Range(0.001,0.5)) = 0.06
        _ShadowColor("Shadow Tint", Color) = (0.25,0.32,0.55,1)
        [Toggle] _UseRampTex("Use Ramp Texture", Float) = 0

        [Header(Face SDF)]
        [Toggle] _UseFaceShadow("Face Shadow Map", Float) = 0
        _FaceShadowMap("Face SDF / Gradient", 2D) = "white" {}
        _FaceShadowSoftness("Face Softness", Range(0.001,0.4)) = 0.08
        _FaceFront("Face Forward OS", Vector) = (0,0,1,0)

        [Header(Hair Aniso)]
        [Toggle] _UseHairAniso("Hair Anisotropic", Float) = 0
        _SpecularColor("Specular", Color) = (1,1,1,1)
        _Smoothness("Smoothness", Range(0,1)) = 0.4
        _AnisoShift("Aniso Shift", Range(-1,1)) = 0.12
        _AnisoShift2("Aniso Shift 2", Range(-1,1)) = -0.08
        _AnisoSpec2("Second Spec Scale", Range(0,2)) = 0.45

        [Header(Rim)]
        _RimColor("Rim Color", Color) = (0.55,0.85,1,1)
        _RimPower("Rim Power", Range(0.5,8)) = 3.5
        _RimIntensity("Rim Intensity", Range(0,2)) = 0.55

        [Header(Outline)]
        _OutlineColor("Outline Color", Color) = (0.05,0.03,0.08,1)
        _OutlineWidth("Outline Width", Range(0,8)) = 1.1

        [HideInInspector] _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        [HideInInspector] _Surface("__surface", Float) = 0
        [HideInInspector] _Blend("__blend", Float) = 0
        [HideInInspector] _Cull("__cull", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "UniversalMaterialType"="Lit"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #pragma shader_feature_local _USEFACESHADOW_ON
            #pragma shader_feature_local _USEHAIRANISO_ON
            #pragma shader_feature_local _USERAMPTEX_ON

            #include "StylizedLitForward.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                return positionCS;
            }

            Varyings ShadowVert(Attributes input)
            {
                Varyings o;
                o.positionCS = GetShadowPositionHClip(input);
                return o;
            }

            half4 ShadowFrag(Varyings i) : SV_TARGET { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask R
            Cull Back
            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };
            Varyings DepthVert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                return o;
            }
            half DepthFrag(Varyings i) : SV_TARGET { return i.positionCS.z; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }
            ZWrite On
            Cull Back
            HLSLPROGRAM
            #pragma vertex DNVert
            #pragma fragment DNFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };
            Varyings DNVert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                return o;
            }
            half4 DNFrag(Varyings i) : SV_TARGET
            {
                float3 n = normalize(i.normalWS);
                return half4(n, 0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode"="Outline" }
            Cull Front
            ZWrite On
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            #include "OutlinePass.hlsl"
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
