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
        _SunDirection  ("Sun Direction", Vector) = (0,1,0,0)
        _UseMainLight  ("Use Main Light Direction", Float) = 1

        // Sun shape controls
        _SunAngularSizeDeg ("Sun Angular Size (deg)", Range(0.1, 10.0)) = 1.0
        _SunFeather        ("Edge Feather", Range(0.0, 1.0)) = 0.1
        _SunRotationDeg    ("Sun Texture Rotation (deg)", Range(0.0, 360.0)) = 0.0

        // Sky rotation to follow sun
        _SkySyncToSun  ("Sky Sync To Sun", Float) = 1
        _SkyBaseSunDir ("Sky Base Sun Dir", Vector) = (0,1,0,0)
        _SkyYawDeg     ("Sky Yaw Deg", Range(0.0, 360.0)) = 0.0
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

            samplerCUBE _SpaceCubemap;

            sampler2D _SunTex;
            float4    _SunTex_ST;

            float4 _SunColor;
            float  _SunIntensity;

            float4 _SunDirection;
            float  _UseMainLight;

            float  _SunAngularSizeDeg;
            float  _SunFeather;
            float  _SunRotationDeg;

            float  _SkySyncToSun;
            float4 _SkyBaseSunDir;
            float  _SkyYawDeg;

            struct Attributes { float3 positionOS : POSITION; };
            struct Varyings   { float4 positionCS: SV_POSITION; float3 directionWS: TEXCOORD0; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(v.positionOS);
                o.directionWS = normalize(worldPos - _WorldSpaceCameraPos.xyz);
                o.positionCS  = TransformObjectToHClip(v.positionOS);
                return o;
            }

            // Build a stable orthonormal basis around n
            void BuildSunBasis(float3 n, out float3 right, out float3 up)
            {
                float3 ref = float3(0,1,0);
                if (abs(dot(ref, n)) > 0.99) ref = float3(1,0,0);
                right = normalize(cross(ref, n));
                up    = normalize(cross(n, right));
            }

            // Rodrigues rotation matrix around axis by angle in radians
            float3x3 RotAxisAngle(float3 axis, float ang)
            {
                float s = sin(ang), c = cos(ang);
                float3 a = normalize(axis);
                float3x3 K = float3x3(
                    0,    -a.z,  a.y,
                    a.z,   0,   -a.x,
                   -a.y,  a.x,   0
                );
                float3x3 I = float3x3(1,0,0, 0,1,0, 0,0,1);
                return I + s*K + (1.0 - c) * mul(K, K);
            }

            // Minimal rotation that maps a to b
            float3x3 RotBetween(float3 a, float3 b)
            {
                float3 v = cross(a, b);
                float c = saturate(dot(a, b)); // avoid slight >1
                float k = 1.0 / (1.0 + c);

                // Handle near opposite
                if (c < 1e-4)
                {
                    // pick any axis orthogonal to a
                    float3 axis = abs(a.x) < 0.9 ? normalize(cross(a, float3(1,0,0))) : normalize(cross(a, float3(0,1,0)));
                    return RotAxisAngle(axis, 3.14159265);
                }

                float3x3 vx = float3x3(
                    0,   -v.z,  v.y,
                    v.z,   0,  -v.x,
                   -v.y,  v.x,   0
                );
                float3x3 I = float3x3(1,0,0, 0,1,0, 0,0,1);
                return I + vx + k * mul(vx, vx);
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

                // Rotate cubemap so that base sun direction lines up with current sun
                float3 sampleDir = viewDir;
                if (_SkySyncToSun > 0.5)
                {
                    float3 baseSun = normalize(_SkyBaseSunDir.xyz);
                    float3x3 Ralign = RotBetween(baseSun, sunDir);

                    // extra yaw around sun axis
                    float3x3 Ryaw = RotAxisAngle(sunDir, radians(_SkyYawDeg));

                    sampleDir = mul(Ryaw, mul(Ralign, sampleDir));
                }

                float4 spaceColor = texCUBE(_SpaceCubemap, sampleDir);

                // only render sun when facing it
                if (dot(viewDir, sunDir) <= 0.0)
                {
                    return spaceColor;
                }

                // Sun disc and texture
                float3 sx, sy;
                BuildSunBasis(sunDir, sx, sy);

                float2 p = float2(dot(viewDir, sx), dot(viewDir, sy));
                float sunRad = radians(_SunAngularSizeDeg);
                float sinR   = max(sin(sunRad), 1e-5);

                // rotate sun texture around sun axis
                float rot = radians(_SunRotationDeg);
                float s = sin(rot), c = cos(rot);
                float2 pRot = float2(c * p.x - s * p.y, s * p.x + c * p.y);

                float2 disc = pRot / sinR;
                float r = length(disc);
                float edge0 = 1.0;
                float edge1 = max(1.0 - _SunFeather, 0.0);
                float mask  = saturate(smoothstep(edge0, edge1, r));
                float2 uv   = disc * 0.5 + 0.5;

                float4 sunSample = tex2D(_SunTex, uv);
                sunSample.rgb *= _SunColor.rgb * _SunIntensity;
                sunSample.a   *= mask;

                float3 finalRGB = spaceColor.rgb + sunSample.rgb * sunSample.a;
                return float4(finalRGB, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Skybox/Cubemap"
}
