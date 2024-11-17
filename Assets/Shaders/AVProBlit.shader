Shader "Hidden/YamaStream/Blit"
{
    Properties
    {
        [HideInInspector] _MainTex ("Blit Texture", 2D) = "black" {}
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Geometry"
            "RenderType" = "Opaque"
            "PreviewType" = "Plane"
        }

        ZTest Always
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "./YamaPlayerShader.cginc"

            sampler2D _MainTex;

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return GetFixedAVProTexture(_MainTex, i.uv);
            }
        ENDCG
        }
    }
}