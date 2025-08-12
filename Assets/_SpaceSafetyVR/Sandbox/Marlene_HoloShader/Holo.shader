Shader "Test/Holo"
{
    Properties
    {
        _BaseTexture (" Base Texture", 2D) = "white" {}
        _BasetextureIntensity ("Base Texture Intensity", Float) = 1.0
        _Density ("StripeDensety", Float) = 100
        _BaseIntensty ("_BaseIntensty", Float) = 0.3
        _Wobble ("_Wobble", Float) = 0.3
        _isFlickering ("_isFlickering", Range(0,40)) =1
        _isOn ("Is On", Range(0,1)) = 1

        _Color ("Main Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Name "Forward"
            Blend  SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float3 ObjPos : TEXCOORD1;
            };

            TEXTURE2D(_BaseTexture);
            SAMPLER(sampler_BaseTexture);

            float _BasetextureIntensity,_Density,_BaseIntensty,_Wobble,_isFlickering,_isOn;

            float4 _Color;
            
            v2f vert (appdata v)
            {  v2f o;

                // Rotation speed in degrees per second (positive = Earth rotation direction)
                // Earth rotates ~360° per 86164 seconds in real life, but let's use a faster one for visibility
                float rotationSpeed = 10.0; // degrees per second
                float angle = radians(_Time.y * rotationSpeed);

                float s = sin(angle);
                float c = cos(angle);

                // Rotation around Y axis
                float3 pos = v.vertex.xyz;
                float3 rotatedPos = float3(
                    pos.x * c - pos.z * s,
                    pos.y,
                    pos.x * s + pos.z * c
                );

                o.ObjPos = rotatedPos;
                o.vertex = TransformObjectToHClip(rotatedPos);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float baseSample = SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, i.uv).r;

                float blendFactor = saturate((baseSample * _BasetextureIntensity)+_BaseIntensty);

               // Slow "noise" from nested sines
              float wobble = sin(i.ObjPos.x * 2 + _Time.y * 0.1) * cos(i.ObjPos.z * 3 + _Time.y * 0.15);
               float wobbleL = lerp(-_Wobble,_Wobble,wobble);

                

                // Scale wobble strength
              //  wobble *= _Wobble; // small displacement

                // Apply wobble to the sine phase
                float wave = 0.5 * sin(_Density * i.ObjPos.y + wobbleL + _Time.y) + 0.5;
                float alpha = lerp(wave,blendFactor,0.8);

                float flicker = 0.5*sin(_Time.y*_isFlickering);
                
                return ((alpha*0.7)+_BaseIntensty) *_Color *(1-flicker)*_isOn;
            }
            ENDHLSL
        }
    }
}
