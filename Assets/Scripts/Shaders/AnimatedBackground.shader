
Shader "Skybox/AnimatedBackground"
{
    Properties
    {
        _AnimationSpeed ("Animation Speed", Float) = 1.0
        _Intensity ("Intensity", Range(0.1, 2.0)) = 1.0
        _TintA ("Tint A", Color) = (0.20, 0.35, 0.55, 1.0)
        _TintB ("Tint B", Color) = (0.85, 0.45, 0.95, 1.0)
    }
    
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 rayDir : TEXCOORD0;
            };

            float _AnimationSpeed;
            float _Intensity;
            float4 _TintA;
            float4 _TintB;

            float3 palette(float t)
            {
                float3 a = float3(0.5, 0.5, 0.5);
                float3 b = float3(0.5, 0.5, 0.5);
                float3 c = float3(1.0, 1.0, 1.0);
                float3 d = float3(0.263, 0.416, 0.557);

                return a + b * cos(6.28318 * (c * t + d));
            }

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.rayDir = mul((float3x3)unity_ObjectToWorld, input.vertex.xyz);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float3 dir = normalize(input.rayDir);

                float2 sphereUV;
                sphereUV.x = atan2(dir.z, dir.x) / (2.0 * 3.14159265) + 0.5;
                sphereUV.y = asin(clamp(dir.y, -1.0, 1.0)) / 3.14159265 + 0.5;

                float2 uv = sphereUV * 2.0 - 1.0;
                uv.x *= _ScreenParams.x / _ScreenParams.y;

                float2 uv0 = uv;
                float3 finalColor = float3(0.0, 0.0, 0.0);
                float currentTime = _Time.y * _AnimationSpeed;

                [unroll]
                for (int i = 0; i < 4; i++)
                {
                    uv = frac(uv * 1.5) - 0.5;

                    float d = length(uv) * exp(-length(uv0));
                    float3 col = palette(length(uv0) + i * 0.4 + currentTime * 0.4);

                    d = sin(d * 8.0 + currentTime) / 8.0;
                    d = abs(d);
                    d = pow(0.01 / max(d, 0.0001), 1.2);

                    finalColor += col * d;
                }

                float3 tintBlend = lerp(_TintA.rgb, _TintB.rgb, saturate(length(uv0)));
                finalColor *= tintBlend;
                finalColor *= _Intensity;

                return float4(finalColor, 1.0);
            }

            ENDCG
        }
    }
}
