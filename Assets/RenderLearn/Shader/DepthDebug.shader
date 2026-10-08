Shader "Custom/DepthDebug"
{
    Properties
    {
        [Enum(RawDepth,0,Linear01Depth,1,LinearEyeDepth,2,worldPosition,3,scanCol,4)]_DebugMode("Debug Mode",Float) = 0
        _EyeDepthRange("EyeDepthDisplayRange",Float) = 20.0
        _WorldPositionScale("World Position Desplay Scale",Float) = 0.1

        _ScanCenterWS("Scan Center WS", Vector) = (0,0,0,0)
        _ScanSpeed("Scan Speed",Float) = 3
        _ScanRadius("Scan Radius", Float) = 5
        _ScanMaxRadius("ScanMaxRadius",float) = 20
        _ScanWidth("Scan Width",Float) = 0.5
        _ScanColor("ScanColor", Color) = (0,1,1,1)
        
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "DepthDebug"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // struct Attributes
            // {
            //     uint vertexID : SV_VertexID;
            // };

            // struct Varyings
            // {
            //     float4 positionCS : SV_POSITION;
            //     float2 uv : TEXCOORD0;
            // };


            CBUFFER_START(UnityPerMaterial)
                float _DebugMode;
                float _EyeDepthRange;
                float _WorldPositionScale;
                float _ScanRadius,_ScanWidth,_ScanMaxRadius,_ScanSpeed;
                float4 _ScanCenterWS;
                half4 _ScanColor;
            CBUFFER_END

            // Varyings vert(Attributes IN)
            // {
            //     Varyings OUT;
            //     OUT.positionCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
            //     OUT.uv = GetFullScreenTriangleTexCoord(IN.vertexID);
            //     return OUT;
            // }

            half4 Frag(Varyings IN) : SV_Target
            {
                float2 screenUV = IN.texcoord.xy;

                float rawDepth = SampleSceneDepth(screenUV);

                float3 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,screenUV).rgb;

                if(_DebugMode < 0.5)
                {
                    return half4(rawDepth,rawDepth,rawDepth,1.0);
                }

                if(_DebugMode < 1.5)
                {
                    float linear01 = Linear01Depth(rawDepth,_ZBufferParams);
                    return half4(linear01,linear01,linear01,1.0);
                }

                if(_DebugMode < 2.5)
                {
                    float eyeDepth = LinearEyeDepth(rawDepth,_ZBufferParams);
                    float eyeDpethDebug = saturate(eyeDepth / max(_EyeDepthRange,0.00001));
                    return half4(eyeDpethDebug,eyeDpethDebug,eyeDpethDebug,1.0);
                }

                if(_DebugMode < 3.5)
                {
                    float3 positionWS = ComputeWorldSpacePosition(screenUV,rawDepth,UNITY_MATRIX_I_VP);
                    float3 debugPosition = positionWS * _WorldPositionScale;
                    return half4(debugPosition,1.0);
                }

           

                if(_DebugMode < 4.5)
                {
                    float3 positionWS = ComputeWorldSpacePosition(screenUV,rawDepth,UNITY_MATRIX_I_VP);

                    float dynamicRadius = fmod(_Time.y * _ScanSpeed, _ScanMaxRadius);

                    float dist = distance(positionWS, _ScanCenterWS.xyz);
                    float ringDistance = abs(dist - dynamicRadius);

                    float ring = 1 - smoothstep(0.0, _ScanWidth, ringDistance);

                    float glow = 1.0 - smoothstep(0.0, _ScanWidth * 4.0, ringDistance);
                    glow *= 0.2;
                 
                    float3 scanColor = _ScanColor * ring * _ScanColor.a + _ScanColor * glow * _ScanColor.a;
                    half3 finalCol = sceneColor + scanColor;
                    return half4(finalCol,1);
                }

                

                return half4(1,1,1,1);
                
            }
            ENDHLSL
        }
    }
}
