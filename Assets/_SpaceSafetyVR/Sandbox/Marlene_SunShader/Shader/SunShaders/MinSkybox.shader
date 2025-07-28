Shader "Skybox/GlobalNoise3D"
{
    Properties
    {
        _NoiseScaleSmol ("Noise Scale", Float) = 1.0
        _NoiseScaleBig ("Noise Scale Big", Float) = 1.0
        _IntensitySmol ("_IntensitySmol", Float) = 1.0
        _IntensityBig ("_IntensityBig", Float) = 1.0
        _CutOffScaleSmol ("cuttoff Scale", Float) = 0.3
        _CutOffScaleBig ("cuttoff Scale Big", Float) = 0.3
        _CutOffScaleBig2 ("cuttoff Scale Big2", Float) = 0.3
        _Tint ("Tint Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off
        ZWrite Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Global only — not in Properties block
            sampler3D _Worley3D;

            float _NoiseScaleSmol,_NoiseScaleBig,_CutOffScale,_CutOffScaleSmol,_CutOffScaleBig,_CutOffScaleBig2,_IntensityBig,_IntensitySmol;
            float4 _Tint;

             float hash( float n ) {
	            return frac(sin(n)*43758.5453);
            }
            
            float Noise( float3 x ) {
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
                float fbm = 0;
                
                    fbm += Noise(p) * amplitude;
                    amplitude *= strength;
                    p         *= frequency;


                    fbm += Noise(p) * amplitude;

                return fbm; 
            }

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.dir = normalize(mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                float3 uvw = dir * 0.5 + 0.5;

                float noiseSmol = hash(uvw*_NoiseScaleSmol);
                noiseSmol = smoothstep(_CutOffScaleSmol,1,(1-noiseSmol.x)) *_IntensitySmol;;
                
                float3 noiseTex = ( tex3D(_Worley3D, uvw * _NoiseScaleBig).rgb);
               float noiseBig = smoothstep(_CutOffScaleBig,1,1-noiseTex.x);
                noiseBig += smoothstep(_CutOffScaleBig2,1,1-noiseTex.y) *_IntensityBig;

                

                float noise = saturate(noiseBig+noiseSmol)*_Tint;
                
                return float4( noiseBig,noiseBig,noiseBig, 1.0);
            }
            ENDCG
        }
    }
    FallBack Off
}


