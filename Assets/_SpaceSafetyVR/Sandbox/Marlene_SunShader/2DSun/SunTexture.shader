Shader "Test/Sun"
{
    Properties
    {
        _SunBaseTexture ("Sun Base Texture", 2D) = "white" {}
        _BasetextureIntensity ("Base Texture Intensity", Float) = 1.0

        _SunRingTexture ("Sun Ring Texture", 2D) = "white" {}
        _RingtextureIntensity ("Ring Texture Intensity", Float) = 1.0

        _Color ("Main Color", Color) = (1,1,1,1)
        _Color1 ("Secondary Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Name "Forward"
            Blend  SrcAlpha OneMinusSrcAlpha
            Cull Back
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
            };

            TEXTURE2D(_SunBaseTexture);
            SAMPLER(sampler_SunBaseTexture);

            TEXTURE2D(_SunRingTexture);
            SAMPLER(sampler_SunRingTexture);

            float _BasetextureIntensity;
            float _RingtextureIntensity;

            float4 _Color;
            float4 _Color1;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float baseSample = SAMPLE_TEXTURE2D(_SunBaseTexture, sampler_SunBaseTexture, i.uv).r;
                float ringSample = SAMPLE_TEXTURE2D(_SunRingTexture, sampler_SunRingTexture, i.uv).r;

                float blendFactor = saturate(baseSample * _BasetextureIntensity);

                float4 finalColor = lerp(_Color1, _Color, blendFactor);

                // Optional: use ringSample for alpha cutout or further blend
                finalColor.a = smoothstep(0.001,0.5, baseSample); // 0 or 1 alpha

                return finalColor+ringSample*_RingtextureIntensity;
            }
            ENDHLSL
        }
    }
}
