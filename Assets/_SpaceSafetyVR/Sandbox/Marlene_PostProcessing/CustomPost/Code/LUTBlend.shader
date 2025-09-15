Shader "Hidden/Custom/LUTBlend"
{
    Properties
    {
        _LUT_A ("LUT A", 2D) = "white" {}
        _LUT_B ("LUT B", 2D) = "white" {}
        _Blend ("Blend", Range(0,1)) = 0
        _ExposureOffset ("Exposure Offset", Float) = 0
        _ContrastBoost ("Contrast Boost", Float) = 1
        _Tint ("Tint", Color) = (1,1,1,1)
        _LutSize("LUT Size", Float) = 32
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_SourceTex);
            SAMPLER(sampler_SourceTex);

            TEXTURE2D(_LUT_A);
            SAMPLER(sampler_LUT_A);
            TEXTURE2D(_LUT_B);
            SAMPLER(sampler_LUT_B);

            float _Blend;
            float _ExposureOffset;
            float _ContrastBoost;
            float4 _Tint;
            float _LutSize;

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

           float3 SampleUnityLUT(float3 color, TEXTURE2D_PARAM(lutTex, lutSampler), float lutSize)
            {
                // Clamp to valid range
                color = saturate(color);

                // Scale color to LUT size
                float slice = color.b * (lutSize - 1.0);
                float sliceLow = floor(slice);
                float sliceHigh = min(sliceLow + 1.0, lutSize - 1.0);
                float interp = slice - sliceLow;

                // Each slice is laid out horizontally in a strip
                float sliceWidth = 1.0 / lutSize;
                float sliceHeight = 1.0 / lutSize;

                // Calculate UVs for low + high slices
                float2 uvLow = (color.rg + float2(sliceLow * lutSize, 0.0)) * sliceWidth;
                float2 uvHigh = (color.rg + float2(sliceHigh * lutSize, 0.0)) * sliceWidth;

                float3 colLow = SAMPLE_TEXTURE2D(lutTex, lutSampler, uvLow).rgb;
                float3 colHigh = SAMPLE_TEXTURE2D(lutTex, lutSampler, uvHigh).rgb;

                return lerp(colLow, colHigh, interp);
            }


            half4 Frag(Varyings i) : SV_Target
            {
                float3 color = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, i.uv).rgb;

                float3 lutA = SampleUnityLUT(color, _LUT_A, sampler_LUT_A, _LutSize);
                float3 lutB = SampleUnityLUT(color, _LUT_B, sampler_LUT_B, _LutSize);
                float3 final = lerp(lutA, lutB, _Blend);


                // Apply VR-safe adjustments
                final = final * pow(2.0, _ExposureOffset);  // exposure
                final = ((final - 0.5) * _ContrastBoost + 0.5); // contrast
                final *= _Tint.rgb;

                return float4(final, 1.0);
            }
            ENDHLSL
        }
    }
}
