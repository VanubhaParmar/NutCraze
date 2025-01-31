Shader "Custom/UI-OuterGlowShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1
        _GlowDistance ("Glow Distance", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _GlowDistance;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 glow = 0;
    
                // Sample in a circular pattern with more samples
                const int SAMPLES = 32;
                const float TWO_PI = 6.28318530718;
    
                for (int j = 0; j < SAMPLES; j++)
                {
                    float angle = TWO_PI * j / SAMPLES;
                    float r = _GlowDistance * (1.0 - float(j) / SAMPLES);
                    float2 offset = float2(cos(angle), sin(angle)) * r;
        
                    float dist = length(offset);
                    float falloff = exp(-dist * dist * 4.0);
        
                    // Sample texture but only use alpha for glow
                    fixed4 sampleColor = tex2D(_MainTex, i.uv + offset);
                    glow += float4(_GlowColor.rgb, sampleColor.a) * falloff;
                }
    
                glow *= _GlowIntensity / SAMPLES;
    
                // Blend white glow with original colored sprite
                return col + glow * (1.0 - col.a);
            } 
            ENDCG
        }
    }
    FallBack "UI/Default"
}
