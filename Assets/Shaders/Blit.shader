Shader "Hidden/YamaStream/Blit"
{
    Properties
    {
        [HideInInspector] _MainTex("Blit Texture", 2D) = "black" {}
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
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;

            v2f vert(appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.texcoord);
                tex.rgb = pow(tex.rgb, 2.2);
                tex.a = 1;
                return tex;
            }
        ENDCG
        }
    }
}