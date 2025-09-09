Shader "Skybox/SpaceWithTexturedSun"
{
    Properties
    {
        _SpaceCubemap ("Space Cubemap", CUBE) = "" {}

        // Sun appearance
        _SunTex        ("Sun Texture", 2D) = "white" {}
        _SunColor      ("Sun Color", Color) = (1,1,0.8,1)
        _SunIntensity  ("Sun Intensity", Range(0,10)) = 1.0

        // Sun placement
        _SunDirection  ("Sun Direction", Vector) = (0,1,0,0) // used if _UseMainLight is 0
        _UseMainLight  ("Use Main Light Direction", Float) = 1

        // Sun shape controls
        _SunAngularSizeDeg ("Sun Angular Size (deg)", Range(0.1, 10.0)) = 1.0
        _SunFeather        ("Edge Feather", Range(0.0, 1.0)) = 0.1
        _SunRotationDeg    ("Sun Texture Rotation (deg)", Range(0.0, 360.0)) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off
        ZWrite Off

        Pass
        {
            Name "SkyboxPass"
            Tags { "LightMode" = "Always" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Inputs
            samplerCUBE _SpaceCubemap;

            sampler2D _SunTex;
            float4    _SunTex_ST;

            float4 _SunColor;
            float  _SunIntensity;

            float4 _SunDirection;
            float  _UseMainLight;

            float  _SunAngularSizeDeg; // apparent radius in degrees
            float  _SunFeather;        // soft edge width
            float  _SunRotationDeg;

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 directionWS : TEXCOORD0;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(v.positionOS);
                // Direction from camera into sky
                o.directionWS = normalize(worldPos - _WorldSpaceCameraPos.xyz);
                o.positionCS  = TransformObjectToHClip(v.positionOS);
                return o;
            }

            // Orthonormal basis around axis n
            void BuildSunBasis(float3 n, out float3 right, out float3 up)
            {
                float3 ref = float3(0,1,0);
                // pick another reference if almost parallel
                if (abs(dot(ref, n)) > 0.99) ref = float3(1,0,0);
                right = normalize(cross(ref, n));
                up    = normalize(cross(n, right));
            }

            float4 frag (Varyings i) : SV_Target
            {
                float3 viewDir = normalize(i.directionWS);

                // Sun direction
                float3 sunDir;
                if (_UseMainLight > 0.5)
                {
                    Light mainLight = GetMainLight();
                    sunDir = normalize(mainLight.direction);
                }
                else
                {
                    sunDir = normalize(_SunDirection.xyz);
                }

                // Background
                float4 spaceColor = texCUBE(_SpaceCubemap, viewDir);

                // Local basis around sun direction
                float3 sx, sy;
                BuildSunBasis(sunDir, sx, sy);

                // Coordinates around sun center
                // p.x and p.y are the sine components for angular displacement from sunDir
                float2 p;
                p.x = dot(viewDir, sx);
                p.y = dot(viewDir, sy);

                // True angular distance uses acos(dot(viewDir, sunDir)), but for sizing the disc
                // we can use sin(theta). The radius in this space is sin(radians(size)).
                float sunRad = radians(_SunAngularSizeDeg);
                float sinR   = sin(sunRad);

                // Rotation around the sun axis for the texture
                float rot = radians(_SunRotationDeg);
                float s = sin(rot), c = cos(rot);
                float2 pRot = float2(c * p.x - s * p.y, s * p.x + c * p.y);

                // Normalized disc coordinates where the sun disc is unit radius
                float2 disc = pRot / max(sinR, 1e-5);

                // Disc mask with feather
                float r = length(disc);
                // r = 1 at the sun edge, use smoothstep for feather
                float edge0 = 1.0;                          // start of fade
                float edge1 = max(1.0 - _SunFeather, 0.0);  // inner value gives softness
                float mask  = saturate(smoothstep(edge0, edge1, r));

                // Build UV from disc coords so the sun texture fills the disc
                // map unit disc to 0..1 with center at 0.5
                float2 uv = disc * 0.5 + 0.5;

                // Sample sun texture, clamp outside
                float4 sunSample = tex2D(_SunTex, uv);
                sunSample.rgb *= _SunColor.rgb * _SunIntensity;
                sunSample.a   *= mask;

                // Premultiply by alpha to avoid fringes
                float3 sunRGB = sunSample.rgb * sunSample.a;

                // Composite over space
                float3 finalRGB = spaceColor.rgb + sunRGB;

                return float4(finalRGB, 1.0);
            }

            ENDHLSL
        }
    }

    FallBack "Skybox/Cubemap"
}
