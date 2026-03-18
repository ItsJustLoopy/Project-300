Shader "UI/InventorySpinningCube"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _CubeColor ("Cube Color", Color) = (1,1,1,1)
        _SpinSpeed ("Spin Speed", Float) = 1.6
        _CubeScale ("Cube Scale", Range(0.4, 1.6)) = 0.95

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _CubeColor;
            float4 _ClipRect;
            float _SpinSpeed;
            float _CubeScale;

            float2 Rotate2D(float2 p, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return float2(c * p.x - s * p.y, s * p.x + c * p.y);
            }

            float2 RayBox(float3 ro, float3 rd, float3 halfExtents)
            {
                float3 inv = 1.0 / rd;
                float3 t0 = (-halfExtents - ro) * inv;
                float3 t1 = (halfExtents - ro) * inv;
                float3 tMin = min(t0, t1);
                float3 tMax = max(t0, t1);

                float nearT = max(max(tMin.x, tMin.y), tMin.z);
                float farT = min(min(tMax.x, tMax.y), tMax.z);
                return float2(nearT, farT);
            }

            float3 BoxNormal(float3 p, float3 halfExtents)
            {
                float3 ap = abs(p / halfExtents);

                if (ap.x > ap.y && ap.x > ap.z)
                    return float3(sign(p.x), 0.0, 0.0);
                if (ap.y > ap.z)
                    return float3(0.0, sign(p.y), 0.0);
                return float3(0.0, 0.0, sign(p.z));
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv * 2.0 - 1.0;
                uv *= _CubeScale;

                /*here im actually scaling ray direction input (uv) instead of the cube,
                so what actually happens is that the uv spread further from the center since they are larger,
                which is like increasing the FOV of the camera, which makes the cube look smaller.
                This means cubescale is inversely proportional to the actual scale of the cube.

                I could change this (something like uv /= _CubeScale) to make it directly proportional,
                but its funnier to leave this here and see if anyone actually notices it. 

                If you come across this and are confused about why cubescale is inverted,
                now you know the reason why, but only if you bothered to check the code.
                */


                float3 ro = float3(0.0, 0.0, 2.3);
                float3 rd = normalize(float3(uv * 1.05, -2.3));

                float t = _Time.y * _SpinSpeed;

                ro.xz = Rotate2D(ro.xz, t);
                rd.xz = Rotate2D(rd.xz, t);

                ro.yz = Rotate2D(ro.yz, t * 0.7);
                rd.yz = Rotate2D(rd.yz, t * 0.7);

                float3 halfExtents = float3(0.62, 0.62, 0.62);
                float2 hit = RayBox(ro, rd, halfExtents);

                if (hit.y < max(hit.x, 0.0))
                    discard;

                float travel = max(hit.x, 0.0);
                float3 p = ro + rd * travel;
                float3 n = BoxNormal(p, halfExtents);

                float3 lightDir = normalize(float3(0.5, 0.8, 0.35));
                float3 viewDir = normalize(-rd);
                float3 halfDir = normalize(lightDir + viewDir);

                float ndotl = saturate(dot(n, lightDir));
                float spec = pow(saturate(dot(n, halfDir)), 24.0);

                float shade = 0.28 + ndotl * 0.72;
                float3 rgb = _CubeColor.rgb * shade + spec * 0.18;
                float alpha = _CubeColor.a * i.color.a;

                fixed4 outColor = fixed4(rgb * i.color.rgb, alpha);

                #ifdef UNITY_UI_CLIP_RECT
                outColor.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(outColor.a - 0.001);
                #endif

                return outColor;
            }
            ENDCG
        }
    }
}