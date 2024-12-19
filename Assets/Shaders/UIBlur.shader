// Made by Appletea and permitted for use in YamaPlayer.
// Origianl file name: Gaussian Blur UI v1.5.shader
// https://appleteaworkshop.booth.pm/items/6365276

Shader "Yamadev/YamaStream/UIBlur"
{
    Properties
    {
        [Header(Common Option)]
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _Blur("Blur", Range(0.001, 10)) = 5
        _Diffusion ("Diffusion", Range(1, 10)) = 3

        [Space(30)]
        [Header(Rendering Option)]
        [Enum(UnityEngine.Rendering.CullMode)]
        _Cull("Cull", Float) = 0                // Off
        [Enum(Off, 0, On, 1)]
        _ZWrite("ZWrite", Float) = 0            // Off

        [Space(30)]
        [Header(Stencil Option)]
        [IntRange] _StencilID ("ID", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]
        _StencilComp ("Compare Mode", int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]
        _StencilPassOp("Pass Operation Mode", int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]
        _StencilFailOp("Fail Operation Mode", int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]
        _StencilZFailOp("ZFail Operation Mode", int) = 0
        [IntRange] _StencilWriteMask ("Write Mask", Range(0, 255)) = 255
        [IntRange] _StencilReadMask ("Read Mask", Range(0, 255)) = 255
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" "CanUseSpriteAtlas"="False" }
        Cull[_Cull]
        Lighting Off
        ZWrite[_ZWrite]
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        GrabPass{ "_GrabTexture" }

        Stencil
        {
	        Ref [_StencilID]
	        Comp [_StencilComp]
	        Pass [_StencilPassOp]
	        Fail [_StencilFailOp]
	        Zfail [_StencilZFailOp]
	        ReadMask[_StencilReadMask]
	        WriteMask[_StencilWriteMask]
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float3 pos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID 
                UNITY_VERTEX_OUTPUT_STEREO
            };

            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform float4 _Color;
            uniform float _Blur;
            uniform float _Diffusion;
            uniform sampler2D _GrabTexture;
            uniform float4 _GrabTexture_TexelSize;

            float Gaussian(float r, float delta)
            {
	            return exp(-(pow(r, 2) / (2 * delta))) / sqrt(2 * UNITY_PI * delta);
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.pos = mul(unity_ObjectToWorld, v.vertex).xyz;

                o.screenPos = ComputeGrabScreenPos(o.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                clip(tex2D(_MainTex, i.uv) - 0.5);

                fixed4 col = fixed4(0, 0, 0, 0);

                float weight_total = 0;

                for (int x = -_Blur; x <= _Blur; x++)
                {
                    for (int y = -_Blur; y <= _Blur; y++)
                    {
                        int r = length(float2(x, y));
                        if (r <= _Blur)
                        {
                            float weight = Gaussian(r, _Blur);

                            //ScreenSpaceの問題で接近するほど大きくズレるため距離値で修正
                            float dist = distance(_WorldSpaceCameraPos, i.pos);
                            col += weight * tex2Dproj(_GrabTexture, i.screenPos + float4(dist * _Diffusion * _GrabTexture_TexelSize * float2(x, y), 0, 0));
                            weight_total += weight;
                        }
                    }
                }

                col /= weight_total;
                
                return col * _Color;
            }
            ENDCG
        }
    }
}
