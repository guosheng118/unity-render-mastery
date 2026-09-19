Shader "RenderingLab/Genshin/Floor"
{
    Properties
    {
        _FloorColor("Floor Color", Color) = (0.02, 0.05, 0.14, 1)

        [Header(Sky Cubemap)]
        [Toggle(_USE_SKYCUBE)] _UseSkyCube("Use Custom Cubemap", Float) = 0
        [NoScaleOffset] _SkyCube("Sky Cubemap", Cube) = "" {}
        _SkyTint("Sky Tint", Color) = (1, 1, 1, 1)
        _SkyIntensity("Sky Intensity", Range(0, 2)) = 1
        _SkyMip("Sky Blur (Mip)", Range(0, 8)) = 4
        _SkyFresnel("Sky Fresnel", Range(0.5, 8)) = 4
        _StarBlur("Star Blur", Range(0,8)) = 1.5

        [Header(Character Planar)]
        _ReflectionTint("Reflection Tint", Color) = (0.55, 0.7, 0.95, 1)
        _ReflectionIntensity("Reflection Intensity", Range(0, 1)) = 0.65
        _Blur("Blur", Range(0, 8)) = 1.5
        _FadeDistance("Fade Distance", Range(0.1, 20)) = 3.5
        _FadePower("Fade Power", Range(0.2, 8)) = 1.8
        _FadeCenter("Fade Center (XZ)", Vector) = (0, 0, 0, 0)
        [NoScaleOffset] _SkyCubeB("Sky CubemapB", Cube) = "" {}
        _SkyBlend("Sky Blend", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct DepthAttributes
        {
            float4 positionOS : POSITION;
        };

        struct DepthVaryings
        {
            float4 positionCS : SV_POSITION;
        };
        ENDHLSL

        Pass
        {
            Name "Floor"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _USE_SKYCUBE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _FloorColor;
                half4 _SkyTint;
                half _SkyIntensity;
                float _SkyMip;
                float _SkyFresnel;
                half4 _ReflectionTint;
                half _ReflectionIntensity;
                float _Blur, _StarBlur;
                float _FadeDistance;
                float _FadePower;
                float4 _FadeCenter;
                float _SkyBlend;
            CBUFFER_END

            TEXTURE2D(_PlanarReflectionTex);
            SAMPLER(sampler_PlanarReflectionTex);
            float4 _PlanarReflectionTex_TexelSize;
            float4 _PlanarSkyTex_TexelSize;

            TEXTURE2D(_PlanarSkyTex);
            SAMPLER(sampler_PlanarSkyTex);

            TEXTURE2D(_PlanarFxTex);
            SAMPLER(sampler_PlanarFxTex);

            TEXTURECUBE(_SkyCube);
            SAMPLER(sampler_SkyCube);

            TEXTURECUBE(_SkyCubeB);
            SAMPLER(sampler_SkyCubeB);

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
            };

            Varyings vert(Attributes IN)
            {
                Varyings o = (Varyings)0;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return o;
            }

            half3 SampleSkyCubemap(float3 R, float mip)
            {
                // #if defined(_USE_SKYCUBE)
                //     return SAMPLE_TEXTURECUBE_LOD(_SkyCube, sampler_SkyCube, R, mip).rgb;
                // #else
                //     half4 encoded = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, R, mip);
                //     return DecodeHDREnvironment(encoded, unity_SpecCube0_HDR);
                // #endif
                half3 cubeA = SAMPLE_TEXTURECUBE_LOD(_SkyCube, sampler_SkyCube, R, mip).rgb;
                half3 cubeB = SAMPLE_TEXTURECUBE_LOD(_SkyCubeB, sampler_SkyCubeB, R, mip).rgb;
                return lerp(cubeA, cubeB, saturate(_SkyBlend));
            }


            half3 SamplePlanarSky(float2 uv)
            {
                float2 texel = _PlanarSkyTex_TexelSize.xy * _StarBlur;
                half3 sum = 0;
                [unroll]
                for(int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for(int x = -1; x <= 1; x++)
                    {
                        float2 offset = float2(x, y) * texel;
                        sum += SAMPLE_TEXTURE2D(_PlanarSkyTex, sampler_PlanarSkyTex, uv + offset).rgb;
                    }
                }
                return sum / 9.0;
            }

            half4 SamplePlanar(float2 uv)
            {
                float2 texel = _PlanarReflectionTex_TexelSize.xy * _Blur;
                half4 sum = 0;
                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 offset = float2(x, y) * texel;
                        sum += SAMPLE_TEXTURE2D(_PlanarReflectionTex, sampler_PlanarReflectionTex, uv + offset);
                    }
                }
                return sum / 9.0;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                float3 N = normalize(IN.normalWS);
                float3 V = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                float3 R = reflect(-V, N);

                half3 sky = SampleSkyCubemap(R, _SkyMip) * _SkyTint.rgb * _SkyIntensity;

                float2 uv = GetNormalizedScreenSpaceUV(IN.positionCS);
                half3 starRefl = SamplePlanarSky(uv);
                sky += starRefl;
                sky += SAMPLE_TEXTURE2D(_PlanarFxTex, sampler_PlanarFxTex, uv).rgb;

                half fresnel = pow(saturate(1.0 - saturate(dot(N, V))), _SkyFresnel);
                half3 col = lerp(_FloorColor.rgb, sky, fresnel);

                half4 planar = SamplePlanar(uv);
                float2 delta = IN.positionWS.xz - _FadeCenter.xz;
                float fade = saturate(1.0 - length(delta) / max(_FadeDistance, 1e-4));
                fade = pow(fade, _FadePower);

                half luma = dot(planar.rgb, half3(0.299, 0.587, 0.114));
                half mask = saturate(max(planar.a, luma)) * fade * _ReflectionIntensity;
                col = lerp(col, planar.rgb * _ReflectionTint.rgb, mask);
                return half4(col, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            DepthVaryings DepthOnlyVertex(DepthAttributes IN)
            {
                DepthVaryings o = (DepthVaryings)0;
                o.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return o;
            }

            half4 DepthOnlyFragment(DepthVaryings IN) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
}