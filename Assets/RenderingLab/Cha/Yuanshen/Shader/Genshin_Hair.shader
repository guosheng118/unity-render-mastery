Shader "RenderingLab/Genshin/Hair"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _LightMap("Light Map", 2D) = "white" {}
        _RampMap("Ramp Map", 2D) = "white" {}
        _MetalMap("Metal Map",2D) = "white"{}
        [Toggle] _IsNight("Night Ramp", float) = 0
        
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmooth("Shadow Smooth", Range(0.001, 0.5)) = 0.05

        [Header(SPecular)]
        _SpecExp1("Spec Exp 1", Range(8, 128)) = 64
        // _SpecExp2("Spec Exp 2", Range(8, 128)) = 64
        // _SpecShift1("Spec Shift 1", Range(-0.5,0.5)) = 0.1
        // _SpecShift2("Spec Shift 2", Range(-0.5, 0.5)) = 0.1
        _SpecColor1("Specular Color1", Color) = (1, 1, 1, 1)
        //_SpecColor2("Specular Color2", Color) = (1, 1, 1, 1)
        _SpecIntensity("Specular Intensity", Range(0, 5)) = 1
        _MetalThreshold("Metal Threshold",Range(0.5,1)) = 0.9

        [Header(Rim)]
        _RimWidth("Rim Width", Range(0, 20)) = 4
        _RimScale("Rim Scale", Range(1, 5)) = 1
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimIntensity("Rim Intensity", Range(0, 10)) = 1

        [Header(Outline)]
        _OutlineWidth("Outline Width", Range(0, 5)) = 1
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _Fade("Fade", Range(0, 1)) = 1

        _Glitch("Glitch", Range(0,1)) = 0
        [HDR] _GlitchColor("Glitch Color", Color) = (1,1,1,1)
        _GlitchHeight("Glitch Height", float) = 1
        _GlitchWidth("Glitch Width", Range(0.05,1)) = 0.35
        _PolySize("Poly Size", Range(0.02,0.5)) = 0.15
        _GlowWidth("Glow Width", Range(0.01,0.25)) = 0.06

        [Header(Debug)]
        [Enum(RenderLab.CharaDebugView)] _DebugView("Debug View", float) = 9

    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
        LOD 100


        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _SpecColor1, _SpecColor2, _RimColor;
            half _SpecIntensity;
            float _IsNight;
            half _ShadowThreshold;
            half _ShadowSmooth;
            float _RimWidth, _RimScale, _RimIntensity;
            float _SpecExp1, _SpecExp2, _SpecShift1, _SpecShift2;
            float _MetalThreshold;
            float _OutlineWidth;
            float4 _OutlineColor;
            float _Fade, _Glitch, _GlitchHeight, _GlitchWidth;
            half4 _GlitchColor;
            float _PolySize, _GlowWidth;
            float _DebugView;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "GenShin_Shared.hlsl"

            // CBUFFER_START(UnityPerMaterial)
                //     float4 _BaseMap_ST;
                //     half4 _BaseColor;
                //     half4 _SpecColor;
                //     half _SpecIntensity;
                //     float _IsNight;
                //     half _ShadowThreshold;
                //     half _ShadowSmooth;
                //     float _RimWidth, _RimScale;
                //     float _DebugView;
            // CBUFFER_END

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_LightMap); SAMPLER(sampler_LightMap);
            TEXTURE2D(_RampMap); SAMPLER(sampler_RampMap);
            TEXTURE2D(_MetalMap); SAMPLER(sampler_MetalMap);

            struct Attributes
            {
                float4 positionOS: POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;

            };

            struct Varyings
            {
                float4 positonCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings o = (Varyings)0;

                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normal = GetVertexNormalInputs(IN.normalOS.xyz, IN.tangentOS);

                o.positonCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.normalWS = normal.normalWS;
                o.tangentWS.xyz = normal.tangentWS.xyz;
                o.tangentWS.w = IN.tangentOS.w;
                o.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                return o;

            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                CharaSweepClip(IN.positonCS.xy, _Fade, IN.positionWS.y, _Glitch, _GlitchHeight, _GlitchWidth);
                float3 T =normalize( IN.tangentWS.xyz);
                float3 N = normalize(IN.normalWS);
                float3 B = normalize(cross(N, T) * IN.tangentWS.w);
                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);

                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                //lightmap.g:128 = 正常受光, 0 = 强制进阴影；
                half4 ilm = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, IN.uv);

                half foldShadow = saturate(ilm.g * 2);

                half NdotL = saturate(dot(N, L) * 0.5 + 0.5);

                float rampU = smoothstep(
                _ShadowThreshold - _ShadowSmooth,
                _ShadowThreshold + _ShadowSmooth,
                NdotL
                );

                rampU *= foldShadow;

                
                float row = round(ilm.a * 2.0);
                row = clamp(row, 0.0, 1.0);
                float night = step(0.5, _IsNight);
                float rampV = 1.0 - (night * 5.0 + row + 0.5) / 10.0;
                //return half4(rampV.xxx,1);
                
                float2 rampUV = float2(rampU, rampV);
                
                
                
                half3 ramp = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, rampUV).rgb;

                float3 V = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                float3 H = normalize(L + V);
                
                //float3 T1 = shiftHairTangent(T, N, _SpecShift1);
                //float3 T2 = shiftHairTangent(T, N, _SpecShift2);
                //float spec1 = HairStandSpecular(T1, H, _SpecExp1);
                //float spec2 = HairStandSpecular(T2, H, _SpecExp2);

                //half specMask = ilm.r;
                //half3 specCol = (spec1 * _SpecColor1.rgb + spec2 * _SpecColor2.rgb) * _SpecIntensity * specMask * rampU * mainLight.color;
                
                //Spec
                float NdotH = saturate(dot(N, H));

                half metalMask = step(_MetalThreshold, ilm.r);
                float hairSpec = pow(NdotH, _SpecExp1) * ilm.b * (1.0 - metalMask);

                float2 matcapUV = TransformWorldToViewDir(N,true).xy * 0.5 + 0.5;
                half metal = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, matcapUV).r * metalMask;
                
                
                //float spec = pow(NdotH, _SpecExp1);
                half3 specCol = (hairSpec + metal) * _SpecColor1.rgb * _SpecIntensity * rampU * mainLight.color;
                

                //depth rim
                float2 screenUV = GetNormalizedScreenSpaceUV(IN.positonCS);
                float rawDepth = SampleSceneDepth(screenUV);
                float linear01 = Linear01Depth(rawDepth, _ZBufferParams);

                float3 normalVS = TransformWorldToViewDir(N, true);
                float2 offsetDir = normalize(normalVS.xy);
                float2 offsetUV = offsetDir * _RimWidth / _ScreenParams.xy;
                float depthP = LinearEyeDepth(rawDepth, _ZBufferParams);
                float depthQ = LinearEyeDepth(SampleSceneDepth(screenUV + offsetUV), _ZBufferParams);
                float depthRim = saturate(depthQ - depthP) * _RimScale;

                float rimMask = saturate(depthRim) * (1.0 - rampU);
                half3 rimCol = _RimColor.rgb * _RimIntensity * rimMask * mainLight.color;

                half3 finalColor = albedo.rgb * ramp * mainLight.color + rimCol + specCol;
                

                half3 debugColor = CharaLightingDebugColor(_DebugView, albedo.rgb, N, NdotL,
                
                finalColor, ilm.r, ilm.g, ilm.b, ilm.a, specCol,
                screenUV, rawDepth, linear01, depthRim, rimMask,rampU,T,ramp,0.0,
                0.0, 0.0, 0.0, 0.0);

                return half4(debugColor,1);

                
            }

            ENDHLSL
        }


        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment

            #include "Genshin_Outline.hlsl"
            #include "GenShin_Shared.hlsl"

            struct OutlineAttributes
            {
                float4 positionOS: POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 tangentOS : TANGENT;
            };

            struct OutlineVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            OutlineVaryings OutlineVertex(OutlineAttributes IN)
            {
                OutlineVaryings o = (OutlineVaryings)0;

                o.positionCS = GenshinOutlinePositionCS(IN.positionOS.xyz, IN.normalOS.xyz, IN.tangentOS.xyz, _OutlineWidth);
                o.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                o.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                return o;
            }

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            half4 OutlineFragment(OutlineVaryings IN) : SV_TARGET
            {
                CharaSweepClip(IN.positionCS.xy, _Fade, IN.positionWS.y, _Glitch, _GlitchHeight, _GlitchWidth);
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;
                half3 col = GenshinOutlineColor(albedo, _OutlineColor.rgb, 0.5);
                return half4(col, 1);
            }

            ENDHLSL
            
        }

        Pass
        {
            Name "Glitch"
            Tags {"LightMode" = "UniversalForwardOnly"}
            Cull Back
            ZWrite Off
            ZTest Always
            Blend One One

            HLSLPROGRAM
            #pragma vertex GlitchVert
            #pragma fragment GlitchFrag
            #include "GenShin_Shared.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 snappedWS : TEXCOORD1;
            };

            float3 SnapWS(float3 p, float size)
            {
                size = max(size, 1e-4);
                return round(p / size) * size;
            }

            float3 CellHDR(float3 cell)
            {
                float3 h = frac(dot(cell, float3(0.137, 0.269, 0.421)));
                float3 k = float3(1.0, 2.0 / 3.0, 1.0 / 3.0);
                return saturate(abs(frac(h + k) * 6.0 - 3.0) - 1.0);
            }

            Varyings GlitchVert(Attributes IN)
            {
                Varyings o = (Varyings)0;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                float3 snapped = SnapWS(pos.positionWS, _PolySize);
                o.positionCS = TransformWorldToHClip(snapped);
                o.positionWS = pos.positionWS;
                o.snappedWS = snapped;
                return o;
            }

            half4 GlitchFrag(Varyings IN) : SV_TARGET
            {
                CharaDitherClip(IN.positionCS.xy, _Fade);

                float band = 1.0 - saturate(abs(IN.positionWS.y - _GlitchHeight) / max(_GlitchWidth, 1e-4));
                band = smoothstep(0.0, 1.0, band);

                float3 p = IN.positionWS / max(_PolySize, 1e-4);
                float3 d = min(frac(p), 1.0 - frac(p));
                float grid = min(min(d.x, d.y), d.z);
                float edge = 1.0 - saturate(grid / max(_GlowWidth, 1e-4));
                edge = smoothstep(0.0, 1.0, edge);

                float3 cell = round(IN.snappedWS / max(_PolySize, 1e-4));
                float3 cellCol = CellHDR(cell);

                float glow = lerp(0.35, 1.0, edge);
                float hdr = max(max(_GlitchColor.r, _GlitchColor.g), _GlitchColor.b);

                return half4(cellCol * hdr * _Glitch * band * glow, 1);
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
            #include "GenShin_Shared.hlsl"

            struct DepthAttributes
            {
                float4 positionOS: POSITION;
            };

            struct DepthVaryings
            {
                float4 positonCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            DepthVaryings DepthOnlyVertex(DepthAttributes IN)
            {
                DepthVaryings o = (DepthVaryings)0;

                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                o.positonCS = pos.positionCS;
                o.positionWS = pos.positionWS;

                return o;
            }   

            half4 DepthOnlyFragment(DepthVaryings IN) : SV_TARGET
            {
                CharaSweepClip(IN.positonCS.xy, _Fade, IN.positionWS.y, _Glitch, _GlitchHeight, _GlitchWidth);
                return half4(IN.positonCS.z, 0, 0, 1);
            }

            ENDHLSL
        }

    }
    CustomEditor "RenderLab.Editor.GenshinCharacterGui"
}