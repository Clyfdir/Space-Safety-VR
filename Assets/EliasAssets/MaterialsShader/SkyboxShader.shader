Shader"Skybox/SpaceWithSun"
{
    Properties
    {
        _SpaceCubemap ("Space Cubemap", CUBE) = "" {}
        _SunDirection ("Sun Direction", Vector) = (0,1,0,0)
        _SunColor ("Sun Color", Color) = (1,1,0.8,1)
        _SunSize ("Sun Size", Range(0.99, 1.0)) = 0.1
        _SunFalloff ("Sun Falloff", Range(0.00, 0.1)) = 0.01
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
Cull Off

ZWrite Off

        Pass
        {
            Name"SkyboxPass"
            Tags
            {"LightMode"="Always"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            samplerCUBE _SpaceCubemap;
            float4 _SunColor;
            float4 _SunDirection;
            float _SunSize;
            float _SunFalloff;

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 directionWS : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(v.positionOS);
                o.directionWS = normalize(worldPos - _WorldSpaceCameraPos.xyz);

                o.positionCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float3 viewDir = normalize(i.directionWS);

                Light mainLight = GetMainLight();
                float3 sunDir = normalize(mainLight.direction);

                float4 spaceColor = texCUBE(_SpaceCubemap, viewDir);

                float sunDot = max(0.0, dot(viewDir, -sunDir)); 

                float core = step(_SunSize, sunDot);
                float glow = exp(-pow((1.0 - sunDot) / (1.0 - _SunSize), 2.0) / (_SunFalloff * _SunFalloff));
                float sunIntensity = saturate(core + glow);

                float3 finalColor = spaceColor.rgb + _SunColor.rgb * sunIntensity;
                return float4(finalColor, 1.0);
            }



            ENDHLSL
        }
    }
FallBack"Skybox/Cubemap"
}
