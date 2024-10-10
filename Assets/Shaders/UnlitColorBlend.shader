// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'
// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

Shader "Custom/Unlit/ColorBlendWithLight"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        [Enum(None,0,Add,1,Multiply,2, Subtract,3)] _Blend ("Blend mode subset", Int) = 0
        _MainTex ("Texture", 2D) = "white" {}
        _Specular ("Specular Texture", 2D) = "white" {}
        _SpecularIntensity("Specular Map Intensity", float) = 0
        _SpecularColor ("Specular Color", Color) = (1,1,1,1)
        _Light ("Light", Color) = (1,1,1,1)
        _LightIntensity("Light Intensity", float) = 0
    }
    SubShader
    {

        Tags { "RenderType"="Opaque" }
        LOD 100
        Color [_Color]
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #pragma multi_compile_instancing
          
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _Specular;
            float4 _Specular_ST;
            // float4 _Color;
            float _Blend;
            float4 _Light;
            //float _LightIntensity;
            //float _SpecularIntensity;
            float4 _SpecularColor;

            UNITY_INSTANCING_BUFFER_START (MyProperties)
            UNITY_DEFINE_INSTANCED_PROP (float4, _Color)
            UNITY_DEFINE_INSTANCED_PROP (float, _LightIntensity)
            UNITY_DEFINE_INSTANCED_PROP (float, _SpecularIntensity)
            #define _Color_arr MyProperties
            UNITY_INSTANCING_BUFFER_END(MyProperties)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID (v);
                UNITY_TRANSFER_INSTANCE_ID (v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID (i);
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
               
                switch(_Blend){
                    case 1:
                        col = tex2D(_MainTex, i.uv) + UNITY_ACCESS_INSTANCED_PROP (_Color_arr, _Color);
                        break;
                       
                    case 2:
                        col = tex2D(_MainTex, i.uv) * UNITY_ACCESS_INSTANCED_PROP (_Color_arr, _Color);
                        break;
                                                            
                    case 3:
                        col = tex2D(_MainTex, i.uv) - UNITY_ACCESS_INSTANCED_PROP (_Color_arr, _Color);
                        break;
                }

                col = col * (_Light * UNITY_ACCESS_INSTANCED_PROP (_Color_arr, _LightIntensity));
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                // Apply specular map with alpha and light information
                fixed4 specularMap = tex2D(_Specular, i.uv);
                col.rgb += specularMap.rgb * specularMap.a * UNITY_ACCESS_INSTANCED_PROP (_Color_arr, _SpecularIntensity) * _SpecularColor;
                return col;
            }
          
           
            ENDCG
        }
    }
}