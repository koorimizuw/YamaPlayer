Shader "Yamadev/YamaStream/UIScreen"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "black" {}
        // [Toggle] _AVPro("AVPro", Int) = 0
        _AspectRatio ("Aspect Ratio", Float) = 1.77777778
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "./YamaPlayerShader.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            int _AVPro;
            float _AspectRatio;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                return GetTexture(_MainTex, IN.uv, _MainTex_TexelSize, _AspectRatio);
            }
        ENDCG
        }
    }
}