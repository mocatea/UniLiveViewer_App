Shader "Custom/ScrollableTiledShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Base Texture", 2D) = "white" {}
        _UVScrollSpeed ("UV Scroll Speed", Vector) = (0.1, 0.1, 0, 0)
        _Tiling ("Tiling", Vector) = (1, 1, 0, 0)
        [Toggle] _ZWriteEnabled ("ZWrite Enabled", Float) = 1.0
    }
    
    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite [_ZWriteEnabled]

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _MainTex_ST;  // タイリング/オフセット用
                float4 _UVScrollSpeed;
                float4 _Tiling;
            CBUFFER_END

            sampler2D _MainTex;

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                
                // UVの自動スクロールとタイル処理
                float2 uvOffset = _Time.y * _UVScrollSpeed.xy;
                output.uv = input.uv * _Tiling.xy + uvOffset;
                
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float4 baseColor = _BaseColor * tex2D(_MainTex, input.uv);
                return float4(baseColor.rgb, baseColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Particles/Unlit"
}
