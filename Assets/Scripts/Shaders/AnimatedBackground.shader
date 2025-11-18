
Shader "AnimatedBackground.shader"
{
    Properties
    {
        _DarkColor ("Dark Color", Color) = (0.1, 0.1, 0.15, 1)
        _BrightColor ("Bright Color", Color) = (0.7, 0.7, 0.8, 1)
        _AnimationSpeed ("Animation Speed", Float) = 0.5
    }
    
    SubShader 
    {
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // this data is passed from vertex shader to fragment shader
            struct VertexToFragment
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _DarkColor;
            float4 _BrightColor;
            float _AnimationSpeed;

            
            VertexToFragment vert (MeshData input)
            {
                VertexToFragment output;
                
                // convert mesh position to screen position
                output.vertex = UnityObjectToClipPos(input.vertex);
                
                // pass UV coordinates to fragment shader
                output.uv = input.uv;
                return output;
            }
            
            fixed4 frag (VertexToFragment input) : SV_Target
            {
                // get UV coordinates (0-1)
                float2 pixelPosition = input.uv;
                
                float currentTime = _Time.y * _AnimationSpeed;
                
                // horizontal wave moving right
                float horizontalWave = sin(pixelPosition.x * 3.0 + currentTime);
                
                // vertical wave moving up 
                float verticalWave = cos(pixelPosition.y * 2.5 - currentTime * 0.7);
                
                // diagonal wave moving at an angle
                float diagonalWave = sin((pixelPosition.x + pixelPosition.y) * 2.0 + currentTime * 0.5);
                
                float combinedWaves = (horizontalWave + verticalWave + diagonalWave) / 3.0;
                
                // convert from range -1,1 to 0,1 for color blending
                float blendAmount = combinedWaves * 0.5 + 0.5;
                
                float4 finalColor = lerp(_DarkColor, _BrightColor, blendAmount);

                return finalColor;
            }
            ENDCG
        }
    }
}
