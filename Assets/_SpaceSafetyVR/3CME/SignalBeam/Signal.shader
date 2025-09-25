// For the wireframe: https://github.com/Firnox/URP_Wireframe_Shader/blob/main/Assets/WireframeShader.shader
Shader "Custom/Signal"
{
    Properties
    {
        [Header(Main Settings)]
        _Color ("Main Color", Color) = (1,0.6,0.5,1)

        _BaseIntensity ("Base Intensity", Float) = 1
        
        _StripeDensity ("Stripe Density", Float) = 5

        _StripesFactor ("Stripes Factor", Range(0,100)) = 40
        _fadeDist ("_fadeDist ", Range(0,5)) = 2.3
        

        _SlideSpeed ("Slide Speed", Float) = 4
        

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
            float _BaseIntensity, _SlideSpeed, _StripeDensity,_StripesFactor,_fadeDist;
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
                float wave = abs( sin(_StripeDensity * i.pos.z + _Time.y * _SlideSpeed) );

                float fade = 1-smoothstep(0,_fadeDist,abs(i.pos.z));
                
                float alpha =( pow(wave,_StripesFactor))*fade;//smoothstep(0.8,1,wave);
                
                return _Color* alpha*_BaseIntensity; 
            }
            ENDHLSL
        }
    }
}

