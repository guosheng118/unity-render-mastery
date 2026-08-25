#ifndef STYLIZED_LIT_FORWARD_INCLUDED
#define STYLIZED_LIT_FORWARD_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half _BumpScale;
    half _RampThreshold;
    half _RampSmooth;
    half4 _ShadowColor;
    half _UseRampTex;
    half _UseFaceShadow;
    half _FaceShadowSoftness;
    float4 _FaceFront;
    half _UseHairAniso;
    half4 _SpecularColor;
    half _Smoothness;
    half _AnisoShift;
    half _AnisoShift2;
    half _AnisoSpec2;
    half4 _RimColor;
    half _RimPower;
    half _RimIntensity;
    half4 _OutlineColor;
    half _OutlineWidth;
    half _Cutoff;
CBUFFER_END

TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
TEXTURE2D(_RampTex); SAMPLER(sampler_RampTex);
TEXTURE2D(_FaceShadowMap); SAMPLER(sampler_FaceShadowMap);

float _LabDebugMode;

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    float2 staticLightmapUV : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    float4 tangentWS : TEXCOORD3;
    float3 viewDirWS : TEXCOORD4;
    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 5);
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings Vert(Attributes input)
{
    Varyings o = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs nrm = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    o.positionCS = pos.positionCS;
    o.positionWS = pos.positionWS;
    o.normalWS = nrm.normalWS;
    o.tangentWS = float4(nrm.tangentWS, input.tangentOS.w);
    o.viewDirWS = GetWorldSpaceViewDir(pos.positionWS);
    o.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, o.staticLightmapUV);
    OUTPUT_SH(o.normalWS, o.vertexSH);
    return o;
}

half RampNdotL(half ndotl)
{
    half t = smoothstep(_RampThreshold - _RampSmooth, _RampThreshold + _RampSmooth, ndotl);
#if defined(_USERAMPTEX_ON)
    half3 ramp = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, float2(saturate(ndotl * 0.5 + 0.5), 0.5)).rgb;
    return ramp.r;
#else
    return t;
#endif
}

half FaceShadow(float3 lightDirWS, float3 normalWS, float2 uv, float3 tangentWS, float3 bitangentWS)
{
    // Teaching SDF: light in a 2D "face plane". Real ZZZ uses a dedicated face forward + baked SDF.
    float3 front = normalize(TransformObjectToWorldDir(_FaceFront.xyz));
    float3 right = normalize(cross(float3(0, 1, 0), front));
    float2 lp = float2(dot(lightDirWS, right), dot(lightDirWS, float3(0, 1, 0)));
    lp = lp * 0.5 + 0.5;
    half sdf = SAMPLE_TEXTURE2D(_FaceShadowMap, sampler_FaceShadowMap, uv).r;
    // Without an authored SDF, treat albedo UV.x as a stand-in gradient.
    half faceCoord = lerp(uv.x, sdf, sdf > 0.001 && sdf < 0.999 ? 1 : 0);
    half lit = smoothstep(lp.x - _FaceShadowSoftness, lp.x + _FaceShadowSoftness, faceCoord);
    return lit;
}

float3 ShiftTangent(float3 T, float3 N, float shift)
{
    return normalize(T + N * shift);
}

half HairSpec(float3 T, float3 N, float3 V, float3 L, float shift, half power)
{
    float3 t = ShiftTangent(T, N, shift);
    float TdotH = dot(t, normalize(V + L));
    half sinTH = sqrt(saturate(1 - TdotH * TdotH));
    half dirAtten = saturate(TdotH + 1);
    return dirAtten * pow(sinTH, power);
}

half4 Frag(Varyings i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
    float3 N = normalize(i.normalWS);
    float3 T = normalize(i.tangentWS.xyz);
    float3 B = normalize(cross(N, T) * i.tangentWS.w);
    float3 V = normalize(i.viewDirWS);
    float3 positionWS = i.positionWS;

    float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
#if defined(LIGHTMAP_ON)
    half4 shadowMask = SAMPLE_SHADOWMASK(i.staticLightmapUV);
#else
    half4 shadowMask = half4(1, 1, 1, 1);
#endif
    Light mainLight = GetMainLight(shadowCoord, positionWS, shadowMask);
    half3 lightDir = mainLight.direction;
    half ndotl = saturate(dot(N, lightDir) * 0.5 + 0.5); // half lambert
    half ramp = RampNdotL(ndotl);

#if defined(_USEFACESHADOW_ON)
    ramp = FaceShadow(lightDir, N, i.uv, T, B);
#endif

    half shadow = mainLight.shadowAttenuation * mainLight.distanceAttenuation;
    half3 lighting = lerp(_ShadowColor.rgb, mainLight.color, ramp * shadow);

    half3 bakedGI = SAMPLE_GI(i.staticLightmapUV, i.vertexSH, N);
    lighting += bakedGI * albedo.rgb * 0.65;

    half3 spec = 0;
#if defined(_USEHAIRANISO_ON)
    half p1 = lerp(32, 128, _Smoothness);
    half p2 = lerp(8, 48, _Smoothness);
    spec += HairSpec(T, N, V, lightDir, _AnisoShift, p1) * _SpecularColor.rgb * mainLight.color;
    spec += HairSpec(T, N, V, lightDir, _AnisoShift2, p2) * _SpecularColor.rgb * mainLight.color * _AnisoSpec2;
#else
    float3 H = normalize(V + lightDir);
    spec += pow(saturate(dot(N, H)), lerp(8, 64, _Smoothness)) * _SpecularColor.rgb * mainLight.color * _Smoothness;
#endif

    half rim = pow(1.0h - saturate(dot(N, V)), _RimPower) * _RimIntensity;
    half3 rimCol = _RimColor.rgb * rim * (0.35 + 0.65 * shadow);

    half3 add = 0;
#ifdef _ADDITIONAL_LIGHTS
    uint pixelLightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, positionWS, shadowMask);
        half nd = saturate(dot(N, light.direction) * 0.5 + 0.5);
        add += light.color * light.distanceAttenuation * light.shadowAttenuation * RampNdotL(nd) * 0.6;
    LIGHT_LOOP_END
#endif

    half3 color = albedo.rgb * (lighting + add) + spec + rimCol;

    if (_LabDebugMode > 0.5)
    {
        if (_LabDebugMode < 1.5) return half4(albedo.rgb, 1);
        if (_LabDebugMode < 2.5) return half4(ndotl.xxx, 1);
        if (_LabDebugMode < 3.5) return half4(shadow.xxx, 1);
        if (_LabDebugMode < 4.5) return half4(bakedGI, 1);
        return half4(rimCol, 1);
    }

    return half4(color, albedo.a);
}

#endif
