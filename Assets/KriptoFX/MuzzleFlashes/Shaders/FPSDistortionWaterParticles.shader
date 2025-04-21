// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "KriptoFX/FPS_Pack/WaterParticles"
{
    Properties
    {
        [HDR] _TintColor ("Tint Color", Color) = (1, 1, 1, 1)
        _MainTex ("Main Texture (R) CutOut (G)", 2D) = "white" { }
        _BumpMap ("Normalmap", 2D) = "bump" { }
        _BumpAmt ("Distortion", Float) = 10
    }

    Category
    {

        Tags { "Queue" = "Transparent-6" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off


        SubShader
        {
            
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
                    float4 texcoord : TEXCOORD0;
                    half4 color : COLOR;
                    float texcoordBlend : TEXCOORD1;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 vertex : POSITION;
                    float4 uvbump : TEXCOORD1;
                    half4 color : COLOR;

                    half blend : TEXCOORD6;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                sampler2D _BumpMap;

                float _BumpAmt;
                float _ColorStrength;

                float4 _GrabTexture_TexelSize;
                half4 _TintColor;

                float4 _BumpMap_ST;
                float4 _MainTex_ST;


                v2f vert(appdata_t v)
                {
                    v2f o;
                    
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    
                    o.vertex = TransformObjectToHClip(v.vertex.xyz);
                    
                    o.color = v.color;

                 	o.uvbump.xy = v.texcoord.xy * _BumpMap_ST.xy + _BumpMap_ST.zw;
	                o.uvbump.zw = v.texcoord.zw * _BumpMap_ST.xy + _BumpMap_ST.zw;
                    o.blend = v.texcoordBlend;

                    return o;
                }


                half4 frag(v2f i) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(i)

                    half4 bumpTex1 = tex2D(_BumpMap, i.uvbump.xy);
                    half4 bumpTex2 = tex2D(_BumpMap, i.uvbump.zw);
                    half3 bump = UnpackNormal(lerp(bumpTex1, bumpTex2, i.blend));
                    half alphaBump = saturate((0.94 - pow(bump.z, 127)) * 5);

                    if (alphaBump < 0.1) discard;


                    half4 tex = tex2D(_MainTex, i.uvbump.xy);
                    half4 tex2 = tex2D(_MainTex, i.uvbump.zw);
                    tex = lerp(tex, tex2, i.blend);

                    float2 screenPos = i.vertex.xy * _ScreenSize.zw;
                    screenPos.xy += bump.rg * _BumpAmt * i.color.a * alphaBump;
                    half4 grab = float4(SampleCameraColor(screenPos).xyz, 1);

                    half4 emission = grab + tex.a * _TintColor * i.color * i.color.a;
                    emission.a = _TintColor.a * alphaBump ;

                    return saturate(emission);
                }
                ENDHLSL
            }
        }
    }
}