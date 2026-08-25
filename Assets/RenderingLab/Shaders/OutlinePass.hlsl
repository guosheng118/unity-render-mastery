#ifndef OUTLINE_PASS_INCLUDED
#define OUTLINE_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

float _LabOutlineEnabled;

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
};

Varyings OutlineVert(Attributes input)
{
    Varyings o;
    float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 nWS = normalize(TransformObjectToWorldNormal(input.normalOS));
    // Expand in clip space so thickness is more resolution-stable.
    float4 clip = TransformWorldToHClip(posWS);
    float3 clipNormal = mul((float3x3)UNITY_MATRIX_VP, nWS);
    float2 offset = normalize(clipNormal.xy) * (_OutlineWidth * 0.0015) * clip.w;
    clip.xy += offset;
    o.positionCS = clip;
    return o;
}

half4 OutlineFrag(Varyings i) : SV_Target
{
    clip(_LabOutlineEnabled - 0.5);
    return _OutlineColor;
}

#endif
