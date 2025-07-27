Shader "Custom/URP/CropInstancedLit"
{
    Properties
    {
        [MainTexture] _BaseMap       ("Base Map", 2D)    = "white" {}
        [MainColor]   _BaseColor     ("Base Color", Color) = (1,1,1,1)
        _Cutoff       ("Alpha Cutoff", Range(0,1)) = 0.5

        _WindStrength   ("Wind Strength", Float)  = 0.001
        _WindSpeed      ("Wind Speed", Float)     = 0.1
        _WindFrequency  ("Wind Frequency", Float) = 0.3
        _WindDirection  ("Wind Direction", Vector)= (1,0,0,0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "AlphaTest"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _ _SHADOWS_SOFT _ _ADDITIONAL_LIGHTS _ _FORWARD_PLUS
            #pragma multi_compile_instancing
            #pragma target 4.5

            // Core transforms & macros
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // Main lighting & shadows
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
            // Instancing support
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            // Light probes (ambient)
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/AmbientProbe.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseColor;
            float  _Cutoff;

            float _WindStrength;
            float _WindSpeed;
            float _WindFrequency;
            float4 _WindDirection;

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                Varyings OUT;
                float3 pos = IN.positionOS.xyz;

                float heightFactor = saturate(pos.z / 100.0);
                float3 worldPos = TransformObjectToWorld(pos);
                float wave = sin((worldPos.x + worldPos.z) * _WindFrequency + _Time.y * _WindSpeed);

                float3 windDir = normalize(_WindDirection.xyz);
                pos += windDir * wave * _WindStrength * heightFactor;

                worldPos = TransformObjectToWorld(pos);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.positionWS  = worldPos;
                OUT.uv          = IN.uv;
                
                OUT.shadowCoord = TransformWorldToShadowCoord(worldPos);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                clip(baseColor.a - _Cutoff);

                // Recalculate normal from geometry using partial derivatives
                float3 dx = ddx(IN.positionWS);
                float3 dy = ddy(IN.positionWS);
                float3 normalWS = normalize(cross(dy, dx));

                // Prepare lighting input
                InputData inputData;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.shadowCoord = IN.shadowCoord;
                inputData.fogCoord = 0;
                inputData.vertexLighting = half3(0,0,0);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = float2(0,0);
                inputData.shadowMask = float4(1,1,1,1);

                // Main directional light
                Light mainLight = GetMainLight(inputData.shadowCoord);
                half3 direct = LightingLambert(mainLight.color, mainLight.direction, normalWS)
                              * mainLight.shadowAttenuation * baseColor.rgb;

                // Additional lights
                half3 extra = 0;
                uint lightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(lightCount)
                    Light add = GetAdditionalLight(lightIndex, inputData.positionWS, baseColor);
                    extra += LightingLambert(add.color, add.direction, normalWS) * add.distanceAttenuation;
                LIGHT_LOOP_END

                half3 finalColor = direct + inputData.bakedGI * baseColor.rgb + extra;

                //return float4(finalColor, baseColor.a);
                return float4(baseColor.rgb, baseColor.a);

            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }

    FallBack "Hidden/InternalErrorShader"
}
