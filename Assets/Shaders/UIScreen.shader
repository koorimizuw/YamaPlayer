Shader "Yamadev/YamaStream/UIScreen"
{
    Properties
    {
        [PerRendererData] _MainTex("Main Texture", 2D) = "black" {}
        [Toggle] _AVPro("AVPro", Int) = 0
        [Toggle] _Flip("Flip", Int) = 1
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
                Name "Default"

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0

                #include "UnityCG.cginc"
                #include "UnityUI.cginc"

                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord  : TEXCOORD0;
                };

                sampler2D _MainTex;
                float4 _MainTex_TexelSize;
                int _AVPro;
                int _Flip;

                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    OUT.vertex = UnityObjectToClipPos(v.vertex);
                    OUT.texcoord = v.texcoord;
                    OUT.color = v.color;

                    if (_AVPro && _Flip) OUT.texcoord.y = 1 - OUT.texcoord.y;

                    float aspect = _MainTex_TexelSize.z / 1.77777778;
                    if (_MainTex_TexelSize.w > aspect) OUT.texcoord.x = ((OUT.texcoord.x - 0.5) / (aspect / _MainTex_TexelSize.w)) + 0.5;
                    if (_MainTex_TexelSize.w < aspect) OUT.texcoord.y = ((OUT.texcoord.y - 0.5) / (_MainTex_TexelSize.w / aspect)) + 0.5;

                    return OUT;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    fixed4 color = tex2D(_MainTex, IN.texcoord);
                    color *= !any(IN.texcoord < 0 || 1 < IN.texcoord);

                    if (_AVPro) color.rgb = pow(color.rgb, 2.2);

                    return color;
                }
            ENDCG
            }
        }
}