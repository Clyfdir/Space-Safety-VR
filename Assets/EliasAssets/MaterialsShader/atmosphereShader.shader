Shader "Custom/AtmosphereShader"
{
    Properties
    {
        _Color ("Atmosphere Color", Color) = (0.4, 0.6, 1, 1)
        _Intensity ("Scattering Intensity", Float) = 1.0
        _ScatteringPower ("Scattering Power", Float) = 2.0
        _FresnelPower ("Fresnel Power", Float) = 3.0
        _MainLightDirection ("Main Light Direction", Vector) = (0, 1, 0, 0)
        _CustomMainLightColor ("Main Light Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "AtmospherePass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            float4 _Color;
            float _Intensity;
            float _ScatteringPower;
            float _FresnelPower;
            float3 _MainLightDirection;
            float3 _CustomMainLightColor;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float4 worldPos = mul(GetObjectToWorldMatrix(), IN.positionOS);
                OUT.positionHCS = TransformWorldToHClip(worldPos.xyz);
                OUT.worldPos = worldPos.xyz;

                // Calculate correct world normal
                float3 objectNormal = normalize(IN.positionOS.xyz);
                float3 worldNormal = normalize(mul((float3x3)GetObjectToWorldMatrix(), objectNormal));
                OUT.normal = worldNormal;

                return OUT;
            }


            half4 frag(Varyings IN) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);
                float3 lightDir = normalize(_MainLightDirection);
                float3 lightColor = _CustomMainLightColor;

                // Light-facing side (front-lit = 1, back-lit = 0)
                float lightAmount = saturate(dot(IN.normal, lightDir));
                float scattering = pow(lightAmount, _ScatteringPower);

                // Fresnel brightens edges (dot near 0 = edge)
                float fresnel = pow(saturate(dot(viewDir, IN.normal)), _FresnelPower);

                float alpha = scattering * fresnel * _Intensity;
                float3 finalColor = _Color.rgb * lightColor * scattering * _Intensity;

                return float4(finalColor, alpha);
            }

            ENDHLSL
        }
    }
}
