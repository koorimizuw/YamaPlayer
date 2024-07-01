Shader "Yamadev/YamaStream/CRT"
{
    Properties
    {
        [PerRendererData] _MainTex("Main Texture", 2D) = "black" {}
        [Toggle] _AVPro("AVPro", Int) = 0
        [Toggle] _Flip("Flip", Int) = 1
    }
    SubShader
    {
        Lighting Off
        Pass {
            Name "CRT"

            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            int _AVPro;
            int _Flip;

            float4 frag(v2f_customrendertexture i) : SV_Target{
                if (_AVPro && _Flip) i.globalTexcoord.y = 1 - i.globalTexcoord.y;

                float aspect = _MainTex_TexelSize.z / 1.77777778;
                if (_MainTex_TexelSize.w > aspect) i.globalTexcoord.x = ((i.globalTexcoord.x - 0.5) / (aspect / _MainTex_TexelSize.w)) + 0.5;
                if (_MainTex_TexelSize.w < aspect) i.globalTexcoord.y = ((i.globalTexcoord.y - 0.5) / (_MainTex_TexelSize.w / aspect)) + 0.5;

                fixed4 color = tex2D(_MainTex, i.globalTexcoord);
                color *= !any(i.globalTexcoord < 0 || 1 < i.globalTexcoord);

                if (_AVPro) color.rgb = pow(color.rgb, 2.2);

                return color; 
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
