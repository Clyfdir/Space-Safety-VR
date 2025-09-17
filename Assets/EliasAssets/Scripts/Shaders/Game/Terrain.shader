Shader "Custom/URP/SimpleHemisphereBlinnPhong"
{
    Properties
    {
        _ColWest("Color West", 2D) = "white" {}
        _ColEast("Color East", 2D) = "white" {}
        _NormalMapWest("Normal Map West", 2D) = "bump" {}
        _NormalMapEast("Normal Map East", 2D) = "bump" {}
        _OceanCol("Ocean", 2D) = "white" {}
        _EarthLights("Earth Lights", 2D) = "black" {}   
        _PlanetCenter("Planet Center (World)", Vector) = (0,0,0,0)
        _MainLightDirection("Main Light Direction", Vector) = (0,0,0,0)
        _LightIntensity("Light Intensity", Float) = 1.5
        _AmbientIntensity("Ambient Intensity", Float) = 0.2
        _CityLightIntensity("City Light Intensity", Float) = 2.0
        _CityLightColor("City Light Color", Color) = (1,0.8,0.6,1) // warm yellow-orange
        _CityLightCutoff("City Light Cutoff", Range(0.001,0.3)) = 0.1
        _BlackoutStrength("Blackout Strength", Range(0,1)) = 0.0
        _NoiseScale("Noise Scale", Float) = 50.0
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
            TEXTURE2D(_EarthLights); SAMPLER(sampler_EarthLights);

            float3 _PlanetCenter;
            float4 _MainLightDirection;
            float _LightIntensity;
            float _AmbientIntensity;
            float _CityLightIntensity;
            float4 _CityLightColor;
            float _CityLightCutoff;
            float _BlackoutStrength;
            float _NoiseScale;

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

            // --- smooth hash + value noise (continuous) ---
            // small, cheap pseudo-random for a point
            float hash21(float2 p)
            {
                // a cheap hash based on sin/dot (continuous per cell when interpolated)
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            // 2D value noise: interpolate random corner values
            float valueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                // smoothstep-like interpolation (Hermite)
                float2 u = f * f * (3.0 - 2.0 * f);
                float res = lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
                return res;
            }

            // fractal / layered noise for graininess
            float noiseGrainy(float2 uv)
            {
                // combine several octaves for denser grain
                float n = 0.0;
                n += 0.5  * valueNoise(uv * 1.0);
                n += 0.25 * valueNoise(uv * 2.0);
                n += 0.125 * valueNoise(uv * 4.0);
                n += 0.0625* valueNoise(uv * 8.0);
                // normalize (sum of weights = 0.9375) -> map to 0..1 roughly
                n = n / (0.5+0.25+0.125+0.0625);
                return n;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float3 dirOS = normalize(i.positionOS); 
                float2 uv = pointToUV(dirOS);

                float4 baseSample;
                float3 normalSample;
                float2 tileUV;

                // ocean + lights UV (rotated fix)
                float2 shiftedUV = uv;
                shiftedUV.x = frac(shiftedUV.x + 0.5);

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

                // Ocean color
                float3 oceanColor = SAMPLE_TEXTURE2D(_OceanCol, sampler_OceanCol, shiftedUV).rgb;

                // Land mask (alpha-based)
                float epsilon = 0.01;
                float landMask = (abs(baseSample.a) < epsilon) ? 0.0 : 1.0;

                // Procedural grainy noise (continuous)
                // control scale with _NoiseScale; bigger = finer grain
                float grain = noiseGrainy(shiftedUV * _NoiseScale);

                // Blackout mask (continuous threshold)
                // We use smoothstep instead of hard step for softer transitions (less aliasing)
                float threshold = 1.0 - _BlackoutStrength;
                float blackoutMask = smoothstep(threshold - 0.15, threshold + 0.01, grain);
                // invert so that higher blackout strength -> more off:
                blackoutMask = 1.0 - blackoutMask;

                // Blend land/ocean
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

                // --- City lights overlay with tint and cutoff ---
                float3 cityLightsTex = SAMPLE_TEXTURE2D(_EarthLights, sampler_EarthLights, shiftedUV).rgb;
                float3 cityLights = cityLightsTex * _CityLightColor.rgb;

                // Fade out in sunlight + blackout procedural noise
                float lightMask = saturate((_CityLightCutoff - NdotL) / _CityLightCutoff);
                cityLights *= lightMask * blackoutMask;

                finalColor += cityLights * _CityLightIntensity;

                return float4(finalColor, 1.0);
            }

            ENDHLSL
        }
    }
}
