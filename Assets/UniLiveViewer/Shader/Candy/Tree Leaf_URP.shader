Shader "UniLiveViewer/Tree Leaf_URP"
{

    Properties
    {
        _MainTex("Base Texture",2D) = "white"{}
        _Amplitude("Amplitude",Float) = 1
        _WaveScale("Wave Scale",Float) = 1
        _WaveSpeed("Wave Speed",Float) = 1
        _WaveExp("Wave Exponent",Float) = 8
    }
    SubShader
    {
        Tags { 
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"   

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Amplitude;
                float _WaveScale;
                float _WaveSpeed;
                float _WaveExp;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                //o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = mul(GetObjectToWorldMatrix(), v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 col = tex2D(_MainTex, i.uv);
                float t = _WaveScale * i.worldPos.y + _WaveSpeed * _Time.y;
                //float1 t = _WaveScale * i.uv.y + _WaveSpeed * _Time.y;
                float amp = pow((1.0f + sin(t)) * 0.5f, _WaveExp) * _Amplitude;
                col += col * amp;

                return col;
            }

            ENDHLSL
        }
    }
}
