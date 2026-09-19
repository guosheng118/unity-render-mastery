Shader "RenderingLab/Genshin/Face"
{
    Properties
    {
        [MainTexture]_BaseTex ("Base Texture", 2D) = "white" {}
        [MainColor]_BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _FaceSDF("Face SDF", 2D) = "white"{}
        _FaceShadowColor("Face Shadow Color", Color) = (0.65,0.55,0.70,1)
        _FaceShadowSmooth("Face Shadow Smooth", Range(0.001,0.2)) = 0.04
        _FaceForwardWS("Face Forward", Vector) = (1,0,0,0)
        
        [Header(LightMap)]
        _HeadLightMap("Head Light Map", 2D) = "white"{}

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
        [Enum(RenderLab.CharaDebugView)]_DebugView("Debug View", float) = 3
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry"}
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _BaseTex_ST;
            float4 _FaceSDF_ST;
            float4 _FaceShadowColor;
            float _FaceShadowSmooth;
            float4 _FaceForwardWS;
            float _OutlineWidth;
            float4 _OutlineColor;
            float _DebugView;
            float _Fade, _Glitch, _GlitchHeight, _GlitchWidth;
            half4 _GlitchColor;
            float _PolySize, _GlowWidth;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode"="UniversalForward"}

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "GenShin_Shared.hlsl"

            TEXTURE2D(_BaseTex); SAMPLER(sampler_BaseTex);
            TEXTURE2D(_FaceSDF); SAMPLER(sampler_FaceSDF);
            TEXTURE2D(_HeadLightMap); SAMPLER(sampler_HeadLightMap);

            struct Attributes
            {
                float4 positionOS: POSITION;
                float3 normalOS : NORMAL;
                float2 uv: TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS: SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                half4 color : COLOR;
            };



            Varyings vert(Attributes IN)
            {
                Varyings o = (Varyings)0;

                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normal = GetVertexNormalInputs(IN.normalOS.xyz);
                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.normalWS = normal.normalWS;
                o.color = IN.color;
                o.uv = TRANSFORM_TEX(IN.uv, _BaseTex);
                return o;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                CharaSweepClip(IN.positionCS.xy, _Fade, IN.positionWS.y, _Glitch, _GlitchHeight, _GlitchWidth);
                float3 N = normalize(IN.normalWS);
                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);

                half3 albedo = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, IN.uv).rgb * _BaseColor.rgb;
                half4 headilm = SAMPLE_TEXTURE2D(_HeadLightMap, sampler_HeadLightMap, IN.uv);

                float2 Lxz = L.xz;
                float lxzLen = length(Lxz);
                Lxz = lxzLen > 1e-4 ? Lxz / lxzLen : float2(1.0,0.0);

                float2 Fxz = _FaceForwardWS.xz;
                float fLen = length(Fxz);
                Fxz = fLen > 1e-4 ? Fxz / fLen : float2(1.0,0.0);
             
                float2 Rxz = float2(-Fxz.y, Fxz.x);

                float front = dot(Lxz, Fxz);; //1=正前，-1=正后
                float threshold = saturate(0.5 - front * 0.5);

                float isRight = step(0.0, dot(Lxz, Rxz));
                float2 sdfUV = IN.uv;
                sdfUV.x = lerp(1.0 - IN.uv.x, IN.uv.x, isRight);

                float sdf = SAMPLE_TEXTURE2D(_FaceSDF, sampler_FaceSDF, sdfUV).r;
                float faceShadow = smoothstep(threshold - _FaceShadowSmooth, threshold + _FaceShadowSmooth, sdf);
                faceShadow = lerp(1.0, faceShadow, headilm.r);
                faceShadow = lerp(faceShadow, 1.0, headilm.a);
                half3 shadowLit = lerp(_FaceShadowColor.rgb, 1.0, faceShadow);

                half3 finalColor = albedo * shadowLit * mainLight.color;
              
                half3 debugColor = CharaLightingDebugColor(_DebugView, albedo, N, 0,  
                                                  finalColor, 0.0, 0.0, 0.0, 0.0, 0.0,
                                                  float2(0.0,0.0), 0.0, 0.0, 0.0, 0.0, faceShadow, 0.0,0.0,sdf,
                                                  headilm.r, headilm.g, headilm.b, headilm.a);

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

                o.positionCS = GenshinOutlinePositionCS(IN.positionOS.xyz, IN.normalOS.xyz,IN.tangentOS.xyz, _OutlineWidth * IN.color.a);
                o.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                o.uv = TRANSFORM_TEX(IN.uv, _BaseTex);

                return o;
            }

            TEXTURE2D(_BaseTex); SAMPLER(sampler_BaseTex);

            

            half4 OutlineFragment(OutlineVaryings IN) : SV_TARGET
            {
                CharaSweepClip(IN.positionCS.xy, _Fade, IN.positionWS.y, _Glitch, _GlitchHeight, _GlitchWidth);
                half3 albedo = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, IN.uv).rgb * _BaseColor.rgb;
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
            Tags {"LightMode"="DepthOnly"}
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex DepthOnlyVert
            #pragma fragment DepthOnlyFrag

            #include "GenShin_Shared.hlsl"

            struct DepthAttributes
            {
                float4 positionOS: POSITION;
            };

            struct DepthVaryings
            {
                float4 positionCS: SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            DepthVaryings DepthOnlyVert(DepthAttributes IN)
            {
                DepthVaryings o = (DepthVaryings)0;
                o.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return o;
            }

            half DepthOnlyFrag(DepthVaryings IN) : SV_TARGET
            {
                CharaSweepClip(IN.positionCS.xy, _Fade, IN.positionWS.y, _Glitch, _GlitchHeight, _GlitchWidth);
                return IN.positionCS.z;
            }
                
            ENDHLSL
        }
    }
}