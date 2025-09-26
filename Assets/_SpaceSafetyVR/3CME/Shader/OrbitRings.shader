// For the wireframe: https://github.com/Firnox/URP_Wireframe_Shader/blob/main/Assets/WireframeShader.shader
Shader "Custom/OrbitRings"
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
        
        _fadeOffset ("_fadeOffset", Range(0,3))= 2.3
        _fadeFactor ("_fadeFactor", Range(0.01,2))= 0.5


        [Header(Animation Settings)]

        _SlideSpeed ("Slide Speed", Float) = 1.0
        

         [Header(InnerRings Settings)]
        [KeywordEnum(Off, On)]
        _Rings ("InnerRngs Mode", Float) = 0

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

            #pragma shader_feature _RINGS_OFF _RINGS_ON

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
            float _BaseIntensity, _SlideSpeed,_Radius, _RingWidth, _StripeDensity,_StripesFactor,_fadeOffset,_fadeFactor;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;

                float3 pos = v.vertex.xyz;

             
                o.vertex = TransformObjectToHClip(pos);
                o.pos = pos;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Stripes
                float wave =_StripesFactor * sin(_StripeDensity * i.pos.x + _Time.y * _SlideSpeed) + 0.5;
               

                float dist = length(i.pos);

                // for variable amount of rings use a sine curve that gets scaled instead of radius
                float ring = abs(_Radius-dist);
                #if _RINGS_ON
                float innerRadius = _Radius/4;
                float ring1 = abs(innerRadius-dist);
                float ring2 = abs(innerRadius*2-dist);
                float ring3 = abs(innerRadius*3-dist);
                #endif
                

                float fade  = saturate((i.pos.x + _fadeOffset )*_fadeFactor);

                float alpha = ( 1-smoothstep(0,_RingWidth, ring));
                #if _RINGS_ON
                alpha +=( 1-smoothstep(0,_RingWidth, ring1)) +( 1-smoothstep(0,_RingWidth, ring2)) +( 1-smoothstep(0,_RingWidth, ring3));
                #endif
                
                alpha *= (1-wave)*fade;
                
                
                return _Color* alpha*_BaseIntensity; 
            }
            ENDHLSL
        }
    }
}

