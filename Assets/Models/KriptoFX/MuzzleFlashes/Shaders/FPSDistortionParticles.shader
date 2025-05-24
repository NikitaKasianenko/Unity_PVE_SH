Shader "KriptoFX/FPS_Pack/Distortion"
{
    Properties
    {
        [HDR]_TintColor ("Tint Color", Color) = (0, 0, 0, 1)
        _BaseTex ("Base (RGB) Gloss (A)", 2D) = "black" { }
        [HDR]_MainColor ("Main Color", Color) = (1, 1, 1, 1)
        _MainTex ("Normalmap & CutOut", 2D) = "black" { }
        _BumpAmt ("Distortion", Float) = 1
        _InvFade ("Soft Particles Factor", Float) = 0.5
    }



    SubShader
    {

        Tags { "Queue" = "Transparent-8" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

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
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : POSITION;
                float2 uvbump : TEXCOORD1;
                half4 color : COLOR;
    
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _BaseTex;
            half4 _TintColor;
            half4 _MainColor;
            float _BumpAmt;
            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = TransformObjectToHClip(v.vertex.xyz);

                o.color = v.color;
               o.uvbump = v.texcoord * _MainTex_ST.xy + _MainTex_ST.zw;

                return o;
            }

      
            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i)
   
		float2 screenPos = i.vertex.xy * _ScreenSize.zw;
                half3 bump = UnpackNormal(tex2D(_MainTex, i.uvbump));
                half alphaBump = saturate((0.94 - pow(bump.z, 127)) * 5);
                screenPos += bump.rg * i.color.a * alphaBump * _BumpAmt;

                half4 grabColor = float4(SampleCameraColor(screenPos).xyz, 1);
                half4 result = _MainColor;
                result.rgb *= grabColor.xyz;
                
                result.a = saturate(result.a * alphaBump);
                if (result.a < 0.01) discard;
                return result;
            }
            ENDHLSL
        }
    }
}