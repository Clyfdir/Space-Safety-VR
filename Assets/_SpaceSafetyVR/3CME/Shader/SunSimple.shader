
Shader "Custom/SunSimple"
{
    Properties
    {
        [Header(Main Settings)]
        _Color ("Main Color", Color) = (1,1,1,1)
        _BaseIntensity ("Base Intensity", Range(1,3)) = 1

        [Header(Fresnel Settings)]
         _FresnelColor ("Fresnel Color", Color) = (1,0.7,0.2,1)
        _FresnelPower ("Fresnel Power", Float) = 1.0


    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex       : SV_POSITION;
                float3 worldPos     : TEXCOORD2;
                float3 worldNormal  : TEXCOORD3;
            };

            // Textures
            TEXTURE2D(_BaseTexture);
            SAMPLER(sampler_BaseTexture);

            // Properties
            float _BaseIntensity, _FresnelFactor, _FresnelPower;
            float4 _Color,_FresnelColor;

            v2f vert (appdata v)
            {
                v2f o;

                float3 pos = v.vertex.xyz;
                float3 normal = v.normal;


                o.worldPos = TransformObjectToWorld(pos);
                o.worldNormal = TransformObjectToWorldNormal(normal);
                o.vertex = TransformWorldToHClip(o.worldPos);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Fresnel effect
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(1.0 - abs(dot(i.worldNormal, viewDir)), _FresnelPower);
                //fresnel *= _FresnelFactor;

                float4 color = lerp(_Color,_FresnelColor,fresnel);
                return saturate(color)*_BaseIntensity;
            }
            ENDHLSL
        }
    }
}

