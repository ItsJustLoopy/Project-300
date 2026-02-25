Shader "Unlit/MenuBackgroundFlat"
{
    Properties
    {
        _AnimationSpeed ("Animation Speed", Float) = 1.0
        _Intensity ("Intensity", Range(0.1, 2.0)) = 1.0
        _Scale ("Scale", Float) = 90.0
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _AnimationSpeed;
            float _Intensity;
            float _Scale;

            #define THICKNESS 0.0
            #define LENGT 0.13
            #define LAYERS 15.0

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            float2 hash12(float p)
            {
                return frac(float2(sin(p * 591.32), cos(p * 391.32)));
            }

            float hash21(float2 n)
            {
                return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
            }

            float2 hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }

            float2x2 makem2(float theta)
            {
                float c = cos(theta);
                float s = sin(theta);
                return float2x2(c, -s, s, c);
            }

            float field1(float2 p, float time)
            {
                float2 n = floor(p) - 0.5;
                float2 f = frac(p) - 0.5;
                float2 o = hash22(n) * 0.35;
                float2 r = -f - o;
                r = mul(makem2(time + hash21(n) * 3.14), r);

                float d = 1.0 - smoothstep(THICKNESS, THICKNESS + 0.09, abs(r.x));
                d *= 1.0 - smoothstep(LENGT, LENGT + 0.02, abs(r.y));

                float d2 = 1.0 - smoothstep(THICKNESS, THICKNESS + 0.09, abs(r.y));
                d2 *= 1.0 - smoothstep(LENGT, LENGT + 0.02, abs(r.x));

                return max(d, d2);
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 p = input.uv - 0.5;
                p.x *= _ScreenParams.x / _ScreenParams.y;

                float mul = (_ScreenParams.x + _ScreenParams.y) / max(_Scale, 0.0001);
                float time = _Time.y * _AnimationSpeed * 3.0;

                float3 col = float3(0.0, 0.0, 0.0);

                [loop]
                for (int i = 0; i < 15; i++)
                {
                    float fi = (float)i;
                    float2 ds = hash12(fi * 2.5) * 0.20;
                    float v = field1((p + ds) * mul, time);
                    float3 tint = sin(ds.x * 5100.0 + float3(1.0, 2.0, 3.5)) * 0.4 + 0.6;
                    col = max(col, v * tint);
                }

                col *= _Intensity;
                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}
