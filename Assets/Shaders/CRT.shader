Shader "Yamadev/YamaStream/CRT"
{
    Properties
    {
        [PerRendererData] _MainTex("Main Texture", 2D) = "black" {}
        // [Toggle] _AVPro("AVPro", Int) = 0
        _AspectRatio ("Aspect Ratio", Float) = 1.77777778
    }
    SubShader
    {
        Lighting Off
        Pass {
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCustomRenderTexture.cginc"
            #include "./YamaPlayerShader.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            int _AVPro;
            float _AspectRatio;

            float4 frag(v2f_customrendertexture i) : SV_Target{
                return GetTexture(_MainTex, i.globalTexcoord, _MainTex_TexelSize, _AspectRatio); 
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
