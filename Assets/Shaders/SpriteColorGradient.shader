Shader "Custom/PerfectCircleGradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _LightCenter ("Light Purple Center", Color) = (0.063,0.0,0.298,1) // #10004c
        _DarkEdge ("Dark Purple Edge", Color) = (0.047,0.0,0.173,1) // #0c002c
        _CircleSize ("Circle Size", Range(0.0, 2.0)) = 1.0
        _Smoothness ("Gradient Smoothness", Range(0.0, 1.0)) = 0.2
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "PreviewType"="Plane"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _LightCenter;
            fixed4 _DarkEdge;
            float _CircleSize;
            float _Smoothness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Correct aspect ratio for perfect circle
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                
                // Center coordinates
                float2 center = float2(0.5, 0.5);
                float2 uv = i.uv - center;
                
                // Apply aspect ratio correction to maintain perfect circle
                uv.x *= aspect;
                
                // Calculate distance for perfect circle
                float dist = length(uv) * _CircleSize;
                
                // Create smooth circular gradient
                float circle = smoothstep(0.0, _Smoothness, dist);
                
                // Blend between colors
                fixed4 col = lerp(_LightCenter, _DarkEdge, circle);
                
                // Apply texture and vertex color
                fixed4 texColor = tex2D(_MainTex, i.uv);
                col *= texColor * i.color;
                
                // Ensure proper alpha
                col.rgb *= col.a;
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}