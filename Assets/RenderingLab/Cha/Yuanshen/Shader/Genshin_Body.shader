Shader "RenderingLab/Yuanshen/Body"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 2
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _LightMap("Light Map", 2D) = "white" {}
        _RampMap("Ramp Map", 2D) = "white" {}
        [Toggle] _IsNight("Night Ramp", Float) = 0

        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmooth("Shadow Smooth", Range(0.001, 0.5)) = 0.05

        // [Header(Specular)]
        // _SpecColor("Specular Color", Color) = (1, 1, 1, 1)
        // _SpecIntensity("Specular Intensity", Range(0, 5)) = 1
        // _MetalThreshold("Hard Metal Threshold", Range(0, 1)) = 0.9
        // [Header(MatCap)]
        // _MetalMap("MatCap", 2D) = "white" {}
        // _MatCapIntensity("MatCap Intensity", Range(0, 4)) = 1.5
        // _MatCapContrast("MatCap Contrast", Range(0.5, 3)) = 1.4

        [Header(Specnew)]
        [ToggleUI]_EnableMatCap("Enable MatCap", Float) = 0
        _MaCapTex("MaCapTex", 2D) = "white" {}
        _MatCapMaskThreshold("MatCapMaskThreshold", Range(0, 1)) = 0.5
        _MatCapDarkColor("MatCapDarkColor", Color) = (1,1,1,1)
        _MatCapLightColor("MatCapLightColor", Color) = (1,1,1,1)
        _SpecCol("SpecColor", Color) = (1, 1, 1, 1)
        _SpecScale("SpecScale", Range(0, 10)) = 1
        _SpecShininess("SpecShininess", Range(8, 256)) = 128
        _SpecThreshold("SpecThreshold", Range(0, 1)) = 0.5
        _SpecSoftness("SpecSoftness", Range(0, 1)) = 0.5
        _SideSpecThreshold("SideSpecThreshold", Range(0, 1)) = 0.5
        _SpecAA("SpecAA", Range(0, 1)) = 0.5
        

        [Header(DepthRim)]
        _RimWidth("Rim Width", Range(0, 20)) = 4
        _RimScale("Rim Scale", Range(1, 5)) = 1
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimIntensity("Rim Intensity", Range(0, 10)) = 1

        [Header(Emission)]
        _EmissionMap("Emission Mask", 2D) = "black" {}
        _EmissionCol("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionIntensity("Emission Intensity", Range(0, 10)) = 1

        [Header(Outline)]
        [ToggleUI]_EnableOutline("Enable Outline", Float) = 1
        _OutlineWidth("Outline Width", Range(0, 5)) = 1
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)

        _Fade("Fade", Range(0, 1)) = 1

        _Glitch("Glitch", Range(0,1)) = 0
        [HDR] _GlitchColor("Glitch COlor",Color) = (1,1,1,1)
        _GlitchHeight("Glitch Height", float) = 1
        _GlitchWidth("Glitch Width", Range(0.05,1)) = 0.35
        _PolySize("Poly Size", Range(0.02,0.5)) = 0.15
        _GlowWidth("Glow Width", Range(0.01,0.25)) = 0.06

        [Header(Debug)]
        [Enum(RenderLab.CharaDebugView)] _DebugView("Debug View", Float) = 3
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            //half4 _SpecColor;
            half4 _RimColor;
            half4 _EmissionCol;
            half4 _OutlineColor;
            //half _SpecIntensity;
            half _ShadowThreshold;
            half _ShadowSmooth;
            //half _MatCapIntensity;
            //half _MatCapContrast;
            float _IsNight;
            //float _MetalThreshold;
            float _RimWidth;
            float _RimScale;
            float _RimIntensity;
            float _EmissionIntensity;
            float _OutlineWidth;
            float _DebugView;
            float _SpecShininess,_SpecSoftness,_SpecAA,_SpecThreshold,_SideSpecThreshold;
            half4 _SpecCol;
            float _SpecScale;
            float _EnableMatCap;
            half4 _MatCapDarkColor,_MatCapLightColor;
            float _MatCapMaskThreshold;
            float _CullMode;
            float _EnableOutline;
            float _Fade, _Glitch, _GlitchHeight, _GlitchWidth;
            half4 _GlitchColor;
            float _PolySize, _GlowWidth;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_CullMode]
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "GenShin_Shared.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_LightMap); SAMPLER(sampler_LightMap);
            TEXTURE2D(_RampMap); SAMPLER(sampler_RampMap);
            //TEXTURE2D(_MetalMap); SAMPLER(sampler_MetalMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_MaCapTex); SAMPLER(sampler_MaCapTex);
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                half4 color : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings o = (Varyings)0;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs nrm = GetVertexNormalInputs(IN.normalOS);
                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.normalWS = nrm.normalWS;
                o.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                o.color = IN.color;
                return o;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                //CharaDitherClip(IN.positionCS.xy, _Fade);
                CharaSweepClip(IN.positionCS.xy, _Fade, IN.positionWS.y, _Glitch, _GlitchHeight, _GlitchWidth);
                float3 N = normalize(IN.normalWS);
                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);

                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;
                half4 ilm = SAMPLE_TEXTURE2D(_LightMap, sampler_LightMap, IN.uv);

                // G: 128 = 正常受光, 0 = 折进阴影
                half foldShadow = saturate(ilm.g * 2.0);
                half NdotL = saturate(dot(N, L) * 0.5 + 0.5);
                float rampU = smoothstep(
                    _ShadowThreshold - _ShadowSmooth,
                    _ShadowThreshold + _ShadowSmooth,
                    NdotL);
                rampU *= foldShadow;

                // A: ramp 行 0-2
                float row = clamp(round(ilm.a * 2.0), 0.0, 2.0);
                float night = step(0.5, _IsNight);
                float rampV = 1.0 - (night * 5.0 + row + 0.5) / 10.0;
                half3 ramp = SAMPLE_TEXTURE2D(_RampMap, sampler_RampMap, float2(rampU, rampV)).rgb;

                float3 V = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                float3 H = normalize(L + V);
                float NdotH = saturate(dot(N, H));
                float3 normalVS = TransformWorldToViewDir(N, true);

                // R: 金属权重。MatCap 换掉金属区的 ramp，暗部才能暗、亮部才能成片
                // float2 matcapUV = normalVS.xy * 0.5 + 0.5;
                // half3 matcap = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, matcapUV).rgb;
                // matcap = pow(matcap, _MatCapContrast);
                // half metalMask = saturate(ilm.r);
                // half3 baseCol = albedo * ramp * mainLight.color;
                // half3 metalCol = albedo * matcap * _MatCapIntensity * lerp(0.3,1.0, rampU);
                // half3 lit = lerp(baseCol, metalCol, metalMask);

                // // B: 布料 Blinn；ilm.r 很高时再加一粒硬金属闪点
                // half hardMetal = step(_MetalThreshold, ilm.r);
                // float spec = pow(NdotH, lerp(8.0, 128.0, ilm.b));
                // spec *= ilm.b * (1.0 - metalMask) * rampU;
                // spec += pow(NdotH, 64.0) * hardMetal * rampU;
                // half3 specCol = spec * _SpecColor.rgb * _SpecIntensity * mainLight.color;

                //Specnew

                //MatCap
                float2 matcapUV = normalVS.xy * 0.5 + 0.5;
                half matCapMask = saturate(ilm.r - _MatCapMaskThreshold);
                //return half4(matCapMask.xxx,1);
                half3 matcap = SAMPLE_TEXTURE2D(_MaCapTex, sampler_MaCapTex, matcapUV).rgb * (ilm.r);
                half3 matcapCol = half3(1.0,1.0,1.0);
        
           
               
                //Spec
                float rawSpecular = pow(NdotH, _SpecShininess);
                float artistWidth = max(_SpecSoftness,0.0001);
                float aawidth = 0.5 * fwidth(rawSpecular) * _SpecAA;
                float edgeWidth = max(artistWidth, aawidth);
                float toonSpecular = smoothstep(_SpecThreshold - edgeWidth, _SpecThreshold + edgeWidth, rawSpecular);
                toonSpecular *= ilm.r;

                half3 specCol = toonSpecular * rampU * _SpecCol.rgb * _SpecScale * mainLight.color;


                half3 baseCol = albedo * ramp * mainLight.color;

                float2 screenUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                float rawDepth = SampleSceneDepth(screenUV);
                float linear01 = Linear01Depth(rawDepth, _ZBufferParams);
                float2 offsetDir = normalize(normalVS.xy);
                float2 offsetUV = offsetDir * _RimWidth / _ScreenParams.xy;
                float depthP = LinearEyeDepth(rawDepth, _ZBufferParams);
                float depthQ = LinearEyeDepth(SampleSceneDepth(screenUV + offsetUV), _ZBufferParams);
                float depthRim = saturate(depthQ - depthP) * _RimScale;
                float rimMask = saturate(depthRim) * (1.0 - rampU);
                half3 rimCol = _RimColor.rgb * _RimIntensity * rimMask * mainLight.color;

                half emitMask = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv).r;
                half3 emission = albedo * emitMask * _EmissionCol.rgb * _EmissionIntensity;

                if(_EnableMatCap > 0.5)
                {
                    half matCapValue = matcap.r;
                    matcapCol = lerp(_MatCapDarkColor.rgb, _MatCapLightColor.rgb, (matCapValue));
                }
                //return half4(matcapCol,1);
                //half vertexColorG = step(0.6, IN.color.g);
                //return IN.color.g;

                half3 finalColor = baseCol * matcapCol + rimCol + emission + specCol;

                //return half4(rimCol,1);

                // half3 debugColor = CharaLightingDebugColor(
                //     _DebugView, albedo, N, NdotL, finalColor,
                //     ilm.r, ilm.g, ilm.b, ilm.a, specCol,
                //     screenUV, rawDepth, linear01, depthRim, rimMask, rampU,
                //     0.0, ramp, 0.0, 0.0, 0.0, 0.0, 0.0);
                return half4(finalColor, 1);
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

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            struct OutlineAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 tangentOS : TANGENT;
                half4 color : COLOR;
            };

            struct OutlineVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                float3 positionWS : TEXCOORD1;
            };

            OutlineVaryings OutlineVertex(OutlineAttributes IN)
            {
                OutlineVaryings o = (OutlineVaryings)0;
                o.color = IN.color;
                o.positionCS = GenshinOutlinePositionCS(IN.positionOS.xyz, IN.normalOS, IN.tangentOS.xyz, _OutlineWidth * IN.color.r * step(0.5, _EnableOutline));
                o.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                o.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                
                return o;
            }

            half4 OutlineFragment(OutlineVaryings IN) : SV_TARGET
            {
                CharaDitherClip(IN.positionCS.xy, _Fade);
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
            Cull [_CullMode]
            ZWrite Off
            ZTest Always
            //Offset -1,-1
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

            float3 GlitchHash(float3 cell)
            {
                float3 p = frac(cell * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.xxy + p.yzz) * p.zyx);
            }

            float3 CellHDR(float3 cell)
            {
                float3 h = frac(dot(cell, float3(0.137,0.269,0.421)));
                float3 k = float3(1.0,2.0/3.0, 1.0/3.0);
                return saturate(abs(frac(h + k) * 6.0 -3.0) -1.0);
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
                float3 ceelCol = CellHDR(cell);

                float glow = lerp(0.35, 1.0, edge);
                float hdr = max(max(_GlitchColor.r, _GlitchColor.g), _GlitchColor.b);
                
                
                return half4(ceelCol * hdr * _Glitch * band * glow, 1);
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
                float4 positionOS : POSITION;
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            DepthVaryings DepthOnlyVertex(DepthAttributes IN)
            {
                DepthVaryings o = (DepthVaryings)0;
                o.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return o;
            }

            half4 DepthOnlyFragment(DepthVaryings IN) : SV_TARGET
            {
                CharaDitherClip(IN.positionCS.xy, _Fade);
                return 0;
            }
            ENDHLSL
        }
    }

    CustomEditor "RenderLab.Editor.GenshinCharacterGui"
}
