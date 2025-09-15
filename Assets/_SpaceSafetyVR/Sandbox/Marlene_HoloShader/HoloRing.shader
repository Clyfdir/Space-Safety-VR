// For the wireframe: https://github.com/Firnox/URP_Wireframe_Shader/blob/main/Assets/WireframeShader.shader
Shader "Custom/HoloRing"
{
    Properties
    {
        [Header(Main Settings)]
        _Color ("Main Color", Color) = (1,0.6,0.5,1)

        _BaseIntensity ("Base Intensity", Float) = 1
        
         _StripeDensity ("Stripe Density", Float) = 100

        _StripesFactor ("Stripes Factor", Range(0,0.5)) = 0.2

        _IsOn ("Is On", Range(0,1)) = 1
        
        _Radius ("Radius",Float) = 1
        _RingWidth ("Ring Width", Range(0,0.5))= 0.1


        [Header(Animation Settings)]
        _Wobble ("Wobble", Float) = 0.3

        _SlideSpeed ("Slide Speed", Float) = 1.0
        


        [Header(Rotation Settings)]
        [KeywordEnum(Off, On)]
        _Rotation ("Rotation Mode", Float) = 0


        [Header(Render Settings)]
        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcBlend ("Source Blend", Float) = 5   // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstBlend ("Destination Blend", Float) = 1   // One
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100

        Blend [_SrcBlend] [_DstBlend]
        Cull Off
        ZWrite Off

        Pass
        {
            Name "Forward"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _ROTATION_OFF _ROTATION_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex       : SV_POSITION;
                float3 pos : TEXCOORD0;
            };

            // Textures
            TEXTURE2D(_BaseTexture);
            SAMPLER(sampler_BaseTexture);

            // Properties
            float _BaseIntensity, _Wobble, _IsOn, _SlideSpeed,_Radius, _RingWidth, _StripeDensity,_StripesFactor;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;

                float3 pos = v.vertex.xyz;

                #if _ROTATION_ON
                float rotationSpeed = 10.0; // degrees per second
                float angle = radians(_Time.y * rotationSpeed);

                float s = sin(angle);
                float c = cos(angle);

                pos = float3(
                    pos.x * c - pos.z * s,
                    pos.y,
                    pos.x * s + pos.z * c
                );
                
                #endif

                o.vertex = TransformObjectToHClip(pos);
                o.pos = pos;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {

                // dist calc
               float dist = length(i.pos);

                float ring = abs(_Radius-dist);

                // Stripes
                float wave =_StripesFactor * sin(_StripeDensity * i.pos.y + _Time.y * _SlideSpeed) + 0.5;
               // wave *= _StripesFactor;
                
              float alpha =( 1-smoothstep(0,_RingWidth, ring))*(1-wave);
                
                return _Color* alpha*_BaseIntensity; 
            }
            ENDHLSL
        }
    }
}

