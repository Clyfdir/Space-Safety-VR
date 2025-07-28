Shader "Test/Curve"
{
    Properties
    {
        _ShrinkFactor ("Shrink Factor", Float) = 0.8
        _ShrinkFactor1 ("Shrink Factor1", Float) = 1.8
        _FresnelPower ("Fresnel Power", Float) = 4.0
        _EdgeMin ("Edge Min Distance", Float) = 0.0
        _EdgeMax ("Edge Max Distance", Float) = 0.2
        _Center ("Mesh Center", Vector) = (0,0,0,0)
        _Color ("Main Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 shrinkedWS : TEXCOORD2;
                float3 shrinkedWS1 : TEXCOORD3;
            };

            // Uniforms
            float _ShrinkFactor,_ShrinkFactor1;
            float _FresnelPower;
            float _EdgeMin;
            float _EdgeMax;
            float4 _Center;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;

                float3 wobble = v.vertex.xyz + 0.014*noise(v.vertex.xyz+ _Time*0.08);
                
                float3 worldPos = mul(unity_ObjectToWorld, float4(wobble,1)).xyz;
                float3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

                // Scale vertex towards center
                float3 centeredOS = wobble - _Center.xyz;
                float3 shrinkedOS = _Center.xyz + centeredOS * _ShrinkFactor;
                float3 shrinkedWS = mul(unity_ObjectToWorld, float4(shrinkedOS, 1.0)).xyz;
                
                float3 shrinkedOS1 = _Center.xyz + centeredOS * _ShrinkFactor * _ShrinkFactor1;
                float3 shrinkedWS1 = mul(unity_ObjectToWorld, float4(shrinkedOS1, 1.0)).xyz;

                o.positionWS = worldPos;
                o.normalWS = worldNormal;
                o.shrinkedWS = shrinkedWS;
                o.shrinkedWS1 = shrinkedWS1;
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.positionWS);
                float fresnel = pow(1.0 - saturate(dot(i.normalWS, viewDir)), _FresnelPower);
                
                float3 viewDirS = normalize(_WorldSpaceCameraPos - i.shrinkedWS);
                float fresnelS = pow(1.0 - saturate(dot(i.normalWS, viewDirS)), _FresnelPower*2);
                fresnelS= fresnelS* step(fresnelS,0.99);
                
               float3 viewDirS1 = normalize(_WorldSpaceCameraPos - i.shrinkedWS1);
                float fresnelS1 = pow(1.0 - saturate(dot(i.normalWS, viewDirS1)), _FresnelPower*2);
                fresnelS1= fresnelS1* step(fresnelS1,0.99);
               
               
                float finalAlpha = _Color.w *(fresnelS1*0.9 + fresnelS*0.9 + fresnel);
                
                float3 finalColor = _Color.xyz;
                return float4(finalColor, finalAlpha);
            }

            ENDHLSL
        }
    }
}
