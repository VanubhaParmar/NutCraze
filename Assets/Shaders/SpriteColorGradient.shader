Shader "Custom/SpriteColorGradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color1 ("Color 1", Color) = (1,0,0,1) // First color
        _Color2 ("Color 2", Color) = (0,1,0,1) // Second color
        _Color3 ("Color 3", Color) = (0,0,1,1) // Third color
        [Enum(None,0,Horizontal,1,Vertical,2,Radial,3)] _GradientType ("Gradient Type", Int) = 0 // 1: Horizontal, 2: Vertical, 3: Radial
        _Rotation ("Rotation", Range(0, 360)) = 0 // Rotation angle in degrees
        _RadialZoom ("Radial Zoom", Range(0.1, 2.0)) = 1.0 // Zoom factor for radial gradient
        _Center ("Center Offset", Range(0, 1.0)) = 0.5 // Center offset for gradients
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Color [_Color1]
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _Color3;
            float _GradientType; // Gradient type
            float _Rotation; // Rotation angle
            float _RadialZoom; // Zoom factor
            float _Center; // Center offset

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;
                float angle = _Rotation * 3.14159 / 180.0; // Convert degrees to radians
                float cosAngle = cos(angle);
                float sinAngle = sin(angle);

                // Apply rotation to UV coordinates
                float2 rotatedUV;
                rotatedUV.x = i.uv.x * cosAngle - i.uv.y * sinAngle;
                rotatedUV.y = i.uv.x * sinAngle + i.uv.y * cosAngle;

                // Prepare colors with transparency check
                fixed4 color1 = (_Color1.a == 0.0) ? fixed4(0.0, 0.0, 0.0, 0.0) : _Color1;
                fixed4 color2 = (_Color2.a == 0.0) ? fixed4(0.0, 0.0, 0.0, 0.0) : _Color2;
                fixed4 color3 = (_Color3.a == 0.0) ? fixed4(0.0, 0.0, 0.0, 0.0) : _Color3;

                if (_GradientType == 1) // Horizontal
                {
                    float adjustX = i.uv.x - _Center;
                    // Blend between color1, color2, and color3 based on the adjusted rotated x-coordinate
                    col = lerp(lerp(color1, color2, adjustX), color3, adjustX);
                    // Adjust alpha based on the horizontal blend
                    col.a = lerp(lerp(color1.a, color2.a, adjustX), color3.a, adjustX);
                }
                else if (_GradientType == 2) // Vertical
                {
                    float adjustY = i.uv.y - _Center;
                    // Blend between color1, color2, and color3 based on the adjusted rotated y-coordinate
                    col = lerp(lerp(color1, color2, adjustY), color3, adjustY);
                    // Adjust alpha based on the vertical blend
                    col.a = lerp(lerp(color1.a, color2.a, adjustY), color3.a, adjustY);
                }
                else if (_GradientType == 3) // Radial
                {
                    float dist = length(i.uv - 0.5) * _RadialZoom; // Apply zoom factor
                    // Blend between color1, color2, and color3 based on the distance from the center
                    if (dist < 0.5)
                    {
                        col = lerp(color1, color2, dist * 2); // Blend between color1 and color2
                    }
                    else
                    {
                        col = lerp(color2, color3, (dist - 0.5) * 2); // Blend between color2 and color3
                    }
                    // Adjust alpha for radial blend
                    float alpha1 = (dist < 0.5) ? color1.a : color2.a;
                    float alpha2 = (dist < 0.5) ? color2.a : color3.a;
                    col.a = lerp(alpha1, alpha2, (dist < 0.5) ? (dist * 2) : ((dist - 0.5) * 2));
                }
                else
                {
                    col = tex2D(_MainTex, i.uv);
                }

                // Ensure final alpha is clamped between 0 and 1
                col.a = max(col.a, 0.0);

                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
