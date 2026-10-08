Shader "Custom/NormalMap"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [NormalMap]_NormalMap("NormalMap",2D) = "white"{}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS: TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD4;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float tangentSign: TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.tangentWS = normalize(TransformObjectToWorldDir(IN.tangentOS.xyz));
                OUT.tangentSign = IN.tangentOS.w * GetOddNegativeScale();
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 T = normalize(IN.tangentWS);
                float3 B = normalize(cross(N,T)) * IN.tangentSign;

                float4 normalSample = SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,IN.uv);
                float3 normalTS = UnpackNormalScale(normalSample,1);

                float3 normalWS_Detail = normalize(normalTS.x * T + normalTS.y * B + normalTS.z * N);

                Light mainLight = GetMainLight();
                float3 lightDirWS = normalize(mainLight.direction);

                float NdotL_Base = saturate(dot(N, lightDirWS));
                float NdotL_NormalMap = saturate(dot(normalWS_Detail,lightDirWS));

                float lambert = IN.uv.x < 0.5? NdotL_Base:NdotL_NormalMap;


                //
                float3 positionWS = normalize(IN.positionWS);
                float3 v = normalize(_WorldSpaceCameraPos - positionWS);
                half NdotV = dot(N,v);
                return half4(1 - NdotV.xxx,1);
                return half4(lambert.xxx,1);

                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return color;
            }
            ENDHLSL
        }
    }
}
