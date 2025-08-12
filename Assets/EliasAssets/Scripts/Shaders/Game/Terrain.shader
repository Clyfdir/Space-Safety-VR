Shader "Custom/URP/SimpleHemisphereBlinnPhong"
{
    Properties
    {
        _ColWest("Color West", 2D) = "white" {}
        _ColEast("Color East", 2D) = "white" {}
        _NormalMapWest("Normal Map West", 2D) = "bump" {}
        _NormalMapEast("Normal Map East", 2D) = "bump" {}
        _OceanCol("Ocean", 2D) = "white" {}
        _PlanetCenter("Planet Center (World)", Vector) = (0,0,0,0)
        _MainLightDirection("Main Light Direction", Vector) = (0,0,0,0)
        _LightIntensity("Light Intensity", Float) = 1.5
        _AmbientIntensity("Ambient Intensity", Float) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #define UNITY_PI 3.14159265

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_ColWest); SAMPLER(sampler_ColWest);
            TEXTURE2D(_ColEast); SAMPLER(sampler_ColEast);
            TEXTURE2D(_NormalMapWest); SAMPLER(sampler_NormalMapWest);
            TEXTURE2D(_NormalMapEast); SAMPLER(sampler_NormalMapEast);
            
            TEXTURE2D(_OceanCol); SAMPLER(sampler_OceanCol);
            float3 _PlanetCenter;
            float4 _MainLightDirection;
            float _LightIntensity;
            float _AmbientIntensity;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float3 positionOS : TEXCOORD4;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;

                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                float3 worldNormal = TransformObjectToWorldNormal(v.normalOS);
                float3 worldTangent = TransformObjectToWorldDir(v.tangentOS.xyz);
                float tangentSign = v.tangentOS.w;
                float3 worldBitangent = cross(worldNormal, worldTangent) * tangentSign;

                o.positionWS = worldPos;
                o.normalWS = normalize(worldNormal);
                o.tangentWS = normalize(worldTangent);
                o.bitangentWS = normalize(worldBitangent);
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.positionOS = v.positionOS.xyz;

                return o;
            }

            float2 pointToUV(float3 pointOnUnitSphere)
            {
                float longitude = atan2(pointOnUnitSphere.x, pointOnUnitSphere.z);
                float latitude = asin(pointOnUnitSphere.y);
                float u = 1.0 - ((longitude / (2.0 * UNITY_PI)) + 0.5);
                float v = (latitude / UNITY_PI) + 0.5;
                return float2(u, v);
            }

            float4 frag(Varyings i) : SV_Target
            {
                float3 dirOS = normalize(i.positionOS); // Sphere-based UV
                float2 uv = pointToUV(dirOS);

                float4 baseSample;
                float3 normalSample;
                float2 tileUV;

                float2 oceanUV = uv;
                oceanUV.x = frac(oceanUV.x + 0.5); // add 0.5 and wrap to [0,1]

                if (uv.x < 0.5)
                {
                    tileUV = float2(uv.x * 2.0, uv.y);
                    baseSample = SAMPLE_TEXTURE2D(_ColEast, sampler_ColEast, tileUV);
                    normalSample = SAMPLE_TEXTURE2D(_NormalMapEast, sampler_NormalMapEast, tileUV).rgb;
                }
                else
                {
                    tileUV = float2((uv.x - 0.5) * 2.0, uv.y);
                    baseSample = SAMPLE_TEXTURE2D(_ColWest, sampler_ColWest, tileUV);
                    normalSample = SAMPLE_TEXTURE2D(_NormalMapWest, sampler_NormalMapWest, tileUV).rgb;
                }
                
                // Sample ocean using shifted UV
                float3 oceanColor = SAMPLE_TEXTURE2D(_OceanCol, sampler_OceanCol, oceanUV).rgb;
                float epsilon = 0.001;
                float landMask = 1.0;

                if (abs(baseSample.r) < epsilon &&
                    abs(baseSample.g) < epsilon &&
                    abs(baseSample.b) < epsilon &&
                    abs(baseSample.a) < epsilon)
                {
                    landMask = 0.0;
                }

                // Blend: where landMask = 1, use land; where 0, use ocean
                float3 baseColor = lerp(oceanColor, baseSample.rgb, landMask);

                // Normal map unpack
                float3 normalTS = normalize(normalSample * 2.0 - 1.0);
                float3x3 TBN = float3x3(i.tangentWS, i.bitangentWS, i.normalWS);
                float3 detailNormalWS = normalize(mul(normalTS, TBN));

                // Combine with sphere normal
                float3 meshNormalWS = normalize(i.positionWS - _PlanetCenter);
                float3 finalNormal = normalize(meshNormalWS * 2.0 + detailNormalWS * 0.3);

                // Lighting
                float3 lightDir = normalize(_MainLightDirection.xyz);
                float NdotL = saturate(dot(finalNormal, lightDir));
                float3 diffuse = baseColor * NdotL * _LightIntensity;
                float3 ambient = baseColor * _AmbientIntensity;
                float3 finalColor = diffuse + ambient;

                return float4(finalColor, 1.0);
            }


            ENDHLSL
        }
    }
}
