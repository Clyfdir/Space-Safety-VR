Shader "Test/FbmSun1"
{
    Properties
    {
        /* ---------- Fresnel & Tint ---------- */
        _ColorFresnel          ("Main Color",      Color) = (1,1,1,1)
        _Color          ("Main Color",      Color) = (1,1,1,1)
        _Color1          ("Main Color",      Color) = (1,1,1,1)
        _Color2          ("Main Color",      Color) = (1,1,1,1)
        _FresnelPower   ("Fresnel Power",   Float) = 4.0

        /* ---------- Worley FBM controls ---------- */
        _GridDims       ("Grid Dimensions", Float) = 6.0
        _NoiseScale     ("Noise Scale",     Float) = 1.0
        _NoiseAmplitude ("Amplitude",       Float) = 1.0
        _NoiseOctaves   ("Octaves",         Float) = 4.0
        _NoiseStrength  ("Gain / Strength", Float) = 0.5
        _NoiseFrequency ("Lacunarity",      Float) = 2.0
        
        _Time1  ("time1", Float) = 1.15
        _Time2  ("time1", Float) = 1.165
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Name "Forward"
            Blend Off
           // ZWrite Off                     
            Cull Back

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            
            float3 Random3(float3 p)
            {
                return frac(sin(float3(
                    dot(p, float3(127.1, 311.7,  74.7)),
                    dot(p, float3(269.5, 183.3, 246.1)),
                    dot(p, float3(113.5, 271.9, 124.6)))
                ) * 43758.5453);
            }

             float hash( float n ) {
	            return frac(sin(n)*43758.5453);
            }
            
            float noise( float3 x ) {
	            // The noise function returns a value in the range -1.0f -> 1.0f
	            float3 p = floor(x);
	            float3 f = frac(x);
                 
	            f = f*f*(3.0-2.0*f);
	            float n = p.x + p.y*57.0 + 113.0*p.z;
                 
	            return lerp(lerp(lerp( hash(n+0.0), hash(n+1.0),f.x),
		               lerp( hash(n+57.0), hash(n+58.0),f.x),f.y),
		               lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
		               lerp( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
            }

            float3 Worley3D(float3 pos, float gridDims)
            {
                float3 cellPos  = pos * gridDims;
                float3 index    = floor(cellPos);
                float3 fractPos = frac(cellPos);

                float F1 = 1.0, F2 = 1.0;

                [loop] for (int z = -1; z <= 1; ++z)
                [loop] for (int y = -1; y <= 1; ++y)
                [loop] for (int x = -1; x <= 1; ++x)
                {
                    float3 neighbor = float3((float)x, (float)y, (float)z);
                    float3 feature  = Random3(index + neighbor);
                    float  dist     = length(neighbor + feature - fractPos);

                    if (dist < F1)       { F2 = F1; F1 = dist; }
                    else if (dist < F2)  { F2 = dist; }
                }

                return float3(F1, F2, F2 - F1); 
            }

            float3 CalculateFbmWorleyNoise
            (
                float3 posWS,
                float  scale,
                float  amplitude,
                float  octaves,
                float  strength,
                float  frequency,
                float  gridDims
            )
            {
                float3 p = posWS * scale;
                float3 fbm = 0;
                
                    fbm += Worley3D(p, gridDims) * amplitude;
                    amplitude *= strength;
                    p         *= frequency;

                    fbm += Worley3D(p, gridDims) * amplitude;
                    amplitude *= strength;
                    p         *= frequency;

                    fbm += Worley3D(p, gridDims) * amplitude;

                return fbm; 
            }
            
            float CalculateFbmNoise
            (
                float3 posWS,
                float  scale,
                float  amplitude,
                float  strength,
                float  frequency
            )
            {
                float3 p = posWS * scale;
                float3 fbm = 0;
                
                    fbm += noise(p) * amplitude;
                    amplitude *= strength;
                    p         *= frequency;

                    fbm += noise(p) * amplitude;
                    amplitude *= strength;
                    p         *= frequency;

                    fbm += noise(p) * amplitude;

                return fbm; 
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex    : SV_POSITION;
                float3 positionW : TEXCOORD0;
                float3 normalW   : TEXCOORD1;
            };

            
            float4 _ColorFresnel,_Color,_Color1,_Color2;
            float  _FresnelPower;
            float  _GridDims;
            float  _NoiseScale;
            float  _NoiseAmplitude;
            float  _NoiseOctaves;
            float  _NoiseStrength;
            float  _NoiseFrequency,_Time1,_Time2;

            v2f vert (appdata v)
            {
                v2f o;
                o.positionW = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normalW   = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                o.vertex    = mul(UNITY_MATRIX_VP, float4(o.positionW, 1.0));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                /* Fresnel  */
                float3 V     = normalize(_WorldSpaceCameraPos - i.positionW);
                float  NdotV = saturate(dot(i.normalW, V));
                float  fres  = pow(1.0 - NdotV, _FresnelPower);

                /* Worley‑FBM noise */
                float3 q = CalculateFbmWorleyNoise(
                                i.positionW,
                                _NoiseScale,
                                _NoiseAmplitude,
                                _NoiseOctaves,
                                _NoiseStrength,
                                _NoiseFrequency,
                                _GridDims);
                
                float2 r;
                r.x = CalculateFbmNoise(
                                 i.positionW+ 2*q+_Time * _Time1,
                                _NoiseScale,
                                _NoiseAmplitude,
                                _NoiseStrength,
                                _NoiseFrequency);
                r.y = CalculateFbmNoise(  i.positionW+3*q+_Time * _Time2,
                                _NoiseScale,
                                _NoiseAmplitude,
                                _NoiseStrength,
                                _NoiseFrequency);
                float f = CalculateFbmNoise( i.positionW+ float3(r,q.z),
                                _NoiseScale,
                                _NoiseAmplitude,
                                _NoiseStrength,
                                _NoiseFrequency);

                fres   = saturate(fres);
                float3 rgb = lerp(_Color,_Color1,clamp(f*f*3,0,1));
                rgb= lerp(rgb,_Color2,clamp(length(r),0,1));
                rgb= lerp(rgb,_Color,clamp(length(r.x),0,1));
                rgb= lerp(rgb,_ColorFresnel,fres);
               // float3 rgb = (_ColorFresnel.rgb* fres) + (fbm.x*_Color + fbm.y*_Color1*0.8 + fbm.z*_Color2*0.8) * inBetween;

                return float4(rgb, 1);
            }
            ENDHLSL
        }
    }
}

