// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Light Beam URP"
{
    Properties
    {
        _Color      ("Base Color", Color)    = (1, 1, 1, 1)
        _MainTex    ("Gradient Texture", 2D) = "white"{}
        _NoiseTex1  ("Noise Texture 1", 2D)  = "white"{}
        _NoiseTex2  ("Noise Texture 2", 2D)  = "white"{}
        _NoiseScale ("Noise Scale", Vector)  = (1, 1, 1, 1)
        _NoiseSpeed ("Noise Speed", Vector)  = (0.1, 0.1, 0.1, 0.1)
    }

    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_instancing
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float3 world_position : TEXCOORD3;
                float3 normal : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;
                float4 _NoiseScale;
                float4 _NoiseSpeed;
            CBUFFER_END

            sampler2D _MainTex;
            sampler2D _NoiseTex1;
            sampler2D _NoiseTex2;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // 頂点変換（URP用の関数）
                o.position = TransformObjectToHClip(v.vertex.xyz);
                
                // UV座標計算
                o.uv0 = TRANSFORM_TEX(v.texcoord, _MainTex);

                // ワールド座標取得（URP用の関数）
                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.world_position = worldPos;
                
                // ノイズ用UV計算
                o.uv1 = worldPos.xy * _NoiseScale.xy + _NoiseSpeed.xy * _Time.y;
                o.uv2 = worldPos.xy * _NoiseScale.zw + _NoiseSpeed.zw * _Time.y;
                
                // 法線変換（URP用の関数）
                o.normal = TransformObjectToWorldNormal(v.normal);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                // カメラ方向の計算（URP用）
                float3 normal = normalize(i.normal);
                float3 camDir = normalize(i.world_position - _WorldSpaceCameraPos.xyz);
                
                // フレネル風のフォールオフ計算
                float falloff = max(abs(dot(camDir, normal)) - 0.4, 0.0);
                falloff = falloff * falloff * 5.0;

                float4 c = _Color;

                // ノイズテクスチャサンプリング
                float n1 = tex2D(_NoiseTex1, i.uv1).r;
                float n2 = tex2D(_NoiseTex2, i.uv2).r;

                // アルファ値の計算
                c.a *= tex2D(_MainTex, i.uv0).a * n1 * n2 * falloff;

                return c;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Particles/Unlit"
}