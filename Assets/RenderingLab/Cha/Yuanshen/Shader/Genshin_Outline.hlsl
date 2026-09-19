#ifndef Genshin_Outline_INCLUDED
#define Genshin_Outline_INCLUDED

float4 GenshinOutlinePositionCS(float3 positionOS, float3 normalOS, float3 smoothNormalOS, float width)
{

    float3 n = (dot(smoothNormalOS, smoothNormalOS) > 0.01)? smoothNormalOS : normalOS;

    float4 posCS = TransformObjectToHClip(positionOS);

    float3 normalWS = TransformObjectToWorldNormal(n);
    float3 normalVS = TransformWorldToViewDir(normalWS);
    
    float2 clipDir = mul((float2x2)UNITY_MATRIX_P, normalVS.xy);


    // float2 pixelDir = clipDir * _ScreenParams.xy;
    // float lenSq = dot(pixelDir,pixelDir);
    // if(lenSq > 1e-10)
    // {
    //     pixelDir *= rsqrt(lenSq);
    //     posCS.xy += pixelDir * width * 2.0 / _ScreenParams.xy * posCS.w;
    // }



    clipDir = (length(clipDir) > 1e-5)? normalize(clipDir) : float2(1.0,0.0);
    
    posCS.xy += clipDir * width * posCS.w * 2/ _ScreenParams.xy;
    return posCS;
}

half3 GenshinOutlineColor(half3 albedo, half3 outlineColor, half tint)
{
    return lerp(outlineColor, albedo * outlineColor, tint);
}



#endif