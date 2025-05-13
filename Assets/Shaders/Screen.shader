Shader "Yamadev/YamaStream/Screen"
{
	Properties
    {
        _MainTex ("Main Texture", 2D) = "black" {}
        _BaseColor ("Base Color", Color) = (0, 0, 0, 0)
        [Toggle] _MirrorFlip ("Mirror Flip", Int) = 1
        // [Toggle] _AVPro ("AVPro Flag", Int) = 0
        _Emission ("Emission Scale", Float) = 1
        _AspectRatio ("Aspect Ratio", Float) = 1.77777778
	}

	SubShader 
    {
		Tags { "RenderType" = "Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0
        #include "./YamaPlayerShader.cginc"

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        float4 _BaseColor;
        int _MirrorFlip;
        int _VRChatMirrorMode;
        int _AVPro;
        float _Emission;
        float _AspectRatio;

	    struct Input {
		    float2 uv_MainTex;
	    };
    
	    void surf (Input IN, inout SurfaceOutputStandard o) {
			float4 videoTex = GetTexture(_MainTex, IN.uv_MainTex, _MainTex_TexelSize, _AspectRatio, _MirrorFlip && _VRChatMirrorMode);
            o.Albedo = _BaseColor.rgb + videoTex.rgb * (1 - _Emission);
            o.Alpha = _BaseColor.a;
            o.Emission = videoTex * _Emission;
		}
		ENDCG
	}
	FallBack "Diffuse"
}