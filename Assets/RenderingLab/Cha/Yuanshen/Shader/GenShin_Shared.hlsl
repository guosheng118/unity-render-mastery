#ifndef GENSHIN_SHARED_INCLUDED
#define GENSHIN_SHARED_INCLUDED

half3 CharaEncodeDireToRGB(half3 dierction)
{
    return saturate(dierction * 0.5 + 0.5);
}

half3 CharaEncodeSignedScalarToRGB(float scalar)
{
    return saturate(scalar);
}

float3 shiftHairTangent(float3 T, float3 N, float shift)
{
    return normalize(T + N * shift);
}

float3 HairStandSpecular(float3 T, float3 H, float exponent)
{
    float TdotH = dot(T, H);
    float sinTH = sqrt(saturate(1.0 - TdotH * TdotH));
    return pow(sinTH, exponent);
}

void CharaDitherClip(float2 posCS, float fade)
{
    clip(fade - InterleavedGradientNoise(posCS,0) - 1e-4);
}

void CharaSweepClip(float2 posCS, float fade, float posY, float glitch, float height, float width)
{
    float reveal = 1.0;
    if (glitch > 1e-4)
        reveal = saturate((posY - height) / max(width, 1e-4) + 0.5);
    clip(fade * reveal - InterleavedGradientNoise(posCS, 0) - 1e-4);
}

half3 CharaLightingDebugColor(
    float _DebugView,
    half3 baseColor,
    half3 normalWS,
    half halfLambert,
    half3 finalColor,
    half ilmR,
    half ilmG,
    half ilmB,
    half ilmA,
    half3 specular,

    //Depth
    float2 screenSpaceUV,
    float rawDepth,
    float linear01,
    float depthRim,
    float rimMask,
    float rampU,
    float3 tangentWS,
    half3 ramp,
    float sdf,
    half headilmR,
    half headilmG,
    half headilmB,
    half headilmA
)
{
    if(_DebugView < .5)
    {
        return baseColor;
    }
    if(_DebugView < 1.5)
    {
        return CharaEncodeDireToRGB(normalWS);
        //return normalWS;
    }
    if(_DebugView < 2.5)
    {
        return halfLambert;
    }
    if(_DebugView < 3.5)
    {
        return finalColor;
    }

    if(_DebugView < 4.5)
    {
        return ilmR;
    }
    if(_DebugView < 5.5)
    {
        return ilmG;
    }
    if(_DebugView < 6.5)
    {
        return ilmB;
    }
    if(_DebugView < 7.5)
    {
        return ilmA;
    }
    if(_DebugView < 8.5)
    {
        return specular;
    }
    if(_DebugView < 9.5)
    {
        return half3(screenSpaceUV, 0);
    }
    if(_DebugView < 10.5)
    {
        return rawDepth;
    }
    if(_DebugView < 11.5)
    {
        return linear01;
    }

    if(_DebugView < 12.5)
    {
        return depthRim;
    }
    if(_DebugView < 13.5)
    {
        //return saturate(half3(1.0-rampU,1.0-rampU,1.0-rampU));
        return rimMask;
    }
    if(_DebugView < 14.5)
    {
        return rampU;
    }
    if(_DebugView < 15.5)
    {
        return CharaEncodeDireToRGB(tangentWS.xyz);
    }
    if(_DebugView < 16.5)
    {
        return half3(ramp);
    }
    if(_DebugView < 17.5)
    {
        return sdf;
    }
    if(_DebugView < 18.5)
    {
        return headilmR;
    }
    if(_DebugView < 19.5)
    {
        return headilmG;
    }
    if(_DebugView < 20.5)
    {
        return headilmB;
    }
    if(_DebugView < 21.5)
    {
        return headilmA;
    }

    return half3(1,0,1);
}

#endif