Shader "Test/sky"
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
        _NoiseStrength  ("Gain / Strength", Float) = 0.5
        _NoiseFrequency ("Lacunarity",      Float) = 2.0
        
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
            Cull Front

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

          

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
                float3 posO   : TEXCOORD2;
            };

             sampler3D _Worley3D;
            float4 _ColorFresnel,_Color,_Color1,_Color2;
            float  _FresnelPower;
            float  _GridDims;
            float  _NoiseScale;
            float  _NoiseAmplitude;
            float  _NoiseStrength;
            float  _NoiseFrequency,_ShrinkFactor;

            v2f vert (appdata v)
            {
                v2f o;
                
                o.positionW = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normalW = normalize(mul(v.normal, (float3x3)UNITY_MATRIX_M));

                o.posO = v.vertex;
                
                o.vertex    = mul(UNITY_MATRIX_VP, float4(o.positionW, 1.0));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                /* Worley‑FBM noise */
                float3 dir = normalize(i.posO);
                float3 uvw = dir * 0.5 + 0.5;
                float3 q = ( tex3D(_Worley3D, uvw ).rgb);
                
                float3 stars = float3(1,1,1)*smoothstep(0.5,1,1-q.x) ;

                return float4(stars, 1);
            }
            ENDHLSL
        }
    }
}

