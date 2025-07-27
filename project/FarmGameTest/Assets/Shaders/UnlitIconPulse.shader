Shader "Custom/URP/UnlitIconPulse"
{
    Properties
    {
        _MainTex    ("Texture",       2D)    = "white" {}
        _Cutoff     ("Alpha Cutoff",  Range(0,1)) = 0.1
        _PulseSpeed ("Pulse Speed",   Float) = 5.0
        _PulseScale ("Pulse Amount",  Float) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"     = "TransparentCutout"
            "Queue"          = "AlphaTest"
        }

        LOD 100

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite      On
            Cull        Off
            Blend       Off
            AlphaToMask On

            HLSLPROGRAM
            #pragma target 4.5
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            // Core transforms & macros (includes TransformObjectToHClip)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // GPU instancing support (defines UNITY_SETUP_INSTANCE_ID(input))
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float  _Cutoff;
                float  _PulseSpeed;
                float  _PulseScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                // Initialize the global instance ID and object‑to‑world matrices
                UNITY_SETUP_INSTANCE_ID(IN);

                Varyings OUT;
                float  s     = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseScale;
                float3 posOS = IN.positionOS.xyz * s;

                // Transform from object -> homogeneous clip space
                OUT.positionHCS = TransformObjectToHClip(posOS);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                clip(col.a - _Cutoff);
                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
