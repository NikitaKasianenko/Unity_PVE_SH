// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "KriptoFX/FPS_Pack/Glass"
{
    Properties
    {
        [HDR]_TintColor ("Tint Color", Color) = (1, 1, 1, 1)
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "black" { }
        _DuDvMap ("DuDv Map", 2D) = "black" { }
        _BumpAmt ("Distortion", Float) = 10
    }

    SubShader
    {
        Tags { "Queue" = "Transparent-6" "RenderType" = "Transparent" }
        Cull Off
        ZWrite On

        Pass
        {


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma target 4.6

            #pragma multi_compile_particles
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/AtmosphericScattering/AtmosphericScattering.hlsl"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : POSITION;
                //float4 screenUV : TEXCOORD0;
                float2 uvbump : TEXCOORD1;
                float2 uvmain : TEXCOORD2;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _DuDvMap;

            float _BumpAmt;
            float _ColorStrength;

            float4 _TintColor;

            float4 _DuDvMap_ST;
            float4 _MainTex_ST;



            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                //o.screenUV = ComputeScreenPos(o.vertex);

                o.color = v.color;
                //o.color.rgb *= ShadeTranslucentLights(v.vertex);


                o.uvbump = v.texcoord * _DuDvMap_ST.xy + _DuDvMap_ST.zw;
                o.uvmain = v.texcoord * _MainTex_ST.xy + _MainTex_ST.zw;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i)
                half3 bump = UnpackNormal(tex2D(_DuDvMap, i.uvbump));
                half alphaBump = saturate((0.94 - pow(bump.z, 127)) * 5);

                float2 screenPos = i.vertex.xy * _ScreenSize.zw;
                
                screenPos.xy += bump.rg * i.color.a * alphaBump * _BumpAmt;
                half4 grab = float4(SampleCameraColor(screenPos).xyz, 1);
                half4 tex = tex2D(_MainTex, i.uvmain) * i.color;

                half4 res = grab + tex * _TintColor * i.color.a;
                res.a = saturate(res.a);
                return res;
            }
            ENDHLSL
        }
    }
}
