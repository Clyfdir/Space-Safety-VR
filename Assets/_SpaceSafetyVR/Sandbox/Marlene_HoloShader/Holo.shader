// For the wireframe: https://github.com/Firnox/URP_Wireframe_Shader/blob/main/Assets/WireframeShader.shader
Shader "Test/Holo"
{
    Properties
    {
        [Header(Main Settings)]
        _Color ("Main Color", Color) = (1,1,1,1)

        _BaseTexture ("Base Texture", 2D) = "white" {}

        _BaseTextureIntensity ("Base Texture Intensity", Float) = 1.0

        _BaseIntensity ("Base Intensity", Float) = 0.3

        _IsOn ("Is On", Range(0,1)) = 1


        [Header(Animation Settings)]
        _Wobble ("Wobble", Float) = 0.3

        _SlideSpeed ("Slide Speed", Float) = 1.0

        _StripeDensity ("Stripe Density", Float) = 100

        _StripesFactor ("Stripes Factor", Float) = 0.8

        _FlickerSpeed ("Flicker Speed", Range(0,40)) = 1


        [Header(Fresnel Settings)]
        _FresnelFactor ("Fresnel Factor", Float) = 0.3

        _FresnelPower ("Fresnel Power", Float) = 1.0


        [Header(Wireframe Settings)]
        [KeywordEnum(Off, On)]
        _Wireframe ("Wireframe Mode", Float) = 0

        _WireframeScale ("Wireframe Scale", Float) = 1.5

        _WireframeFactor ("Wireframe Factor", Float) = 0.5


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

            // Feature toggles
            #pragma shader_feature _WIREFRAME_OFF _WIREFRAME_ON
            #pragma shader_feature _ROTATION_OFF _ROTATION_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 bary   : TEXCOORD1;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex       : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 bary         : TEXCOORD1;
                float3 worldPos     : TEXCOORD2;
                float3 worldNormal  : TEXCOORD3;
            };

            // Textures
            TEXTURE2D(_BaseTexture);
            SAMPLER(sampler_BaseTexture);

            // Properties
            float _BaseTextureIntensity, _StripeDensity, _BaseIntensity, _Wobble, _FlickerSpeed, _IsOn, _SlideSpeed, _StripesFactor, _FresnelFactor, _FresnelPower, _WireframeScale, _WireframeFactor;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;

                float3 pos = v.vertex.xyz;
                float3 normal = v.normal;

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

                normal = float3(
                    normal.x * c - normal.z * s,
                    normal.y,
                    normal.x * s + normal.z * c
                );
                #endif

                o.worldPos = TransformObjectToWorld(pos);
                o.worldNormal = TransformObjectToWorldNormal(normal);
                o.vertex = TransformWorldToHClip(o.worldPos);
                o.uv = v.uv;
                o.bary = v.bary;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Fresnel effect
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(1.0 - abs(dot(i.worldNormal, viewDir)), _FresnelPower);
                fresnel *= _FresnelFactor;

                // Base texture
                float baseSample = SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, i.uv).r;
                baseSample *= _BaseTextureIntensity;

                // Stripes
                float wave = 0.5 * sin(_StripeDensity * i.worldPos.y + _Time.y * _SlideSpeed) + 0.5;
                wave *= _StripesFactor;

                // Wireframe
                float alphaW = 0;
                #if _WIREFRAME_ON
                float3 unitWidth = fwidth(i.bary);
                float3 aliased = smoothstep(float3(0.0,0.0,0.0), unitWidth * _WireframeScale, i.bary);
                alphaW = 1 - min(aliased.x, min(aliased.y, aliased.z));
                alphaW *= _WireframeFactor;
                #endif

                // Combined alpha
                float alpha = saturate(baseSample + wave + fresnel + alphaW);

                // Flicker
                float flicker = 0.5 * sin(_Time.y * _FlickerSpeed);

                return ((alpha * 0.7) + _BaseIntensity) * _Color * (1 - flicker) * _IsOn;
            }
            ENDHLSL
        }
    }
}

