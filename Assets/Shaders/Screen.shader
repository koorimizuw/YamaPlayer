Shader "Yamadev/YamaStream/Screen"
{
	Properties{
    _MainTex ("Main Texture", 2D) = "black" {}
    _BaseColor ("Base Color", Color) = (0, 0, 0, 0)
    [Toggle] _InversionInMirror("Inversion in mirron", Int) = 1
    [Toggle] _AVPro("AVPro", Int) = 0
    [Toggle] _Flip("Flip", Int) = 1
    _Emission ("Emission Scale", Float) = 1
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.0

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
	    fixed4 _BaseColor;
        int _InversionInMirror;
        int _AVPro;
        int _Flip;
        fixed _Emission;

	    struct Input {
		    float2 uv_MainTex;
	    };

        void vert (inout appdata_full v) {
            if (_AVPro && _Flip) v.texcoord.y = 1 - v.texcoord.y;

            bool inMirror = 0 < dot(cross(UNITY_MATRIX_V[0], UNITY_MATRIX_V[1]), UNITY_MATRIX_V[2]);
            if (inMirror && _InversionInMirror) v.texcoord.xy = float2(1 - v.texcoord.x, v.texcoord.y);

            float aspect = _MainTex_TexelSize.z / 1.77777778;
            if (_MainTex_TexelSize.w > aspect) v.texcoord.x = ((v.texcoord.x - 0.5) / (aspect / _MainTex_TexelSize.w)) + 0.5;
            if (_MainTex_TexelSize.w < aspect) v.texcoord.y = ((v.texcoord.y - 0.5) / (_MainTex_TexelSize.w / aspect)) + 0.5;
        }
    
	    void surf (Input IN, inout SurfaceOutputStandard o) {
            o.Albedo = _BaseColor;

			    fixed4 e = tex2D (_MainTex, IN.uv_MainTex);
            e *= !any(IN.uv_MainTex < 0 || 1 < IN.uv_MainTex);
            if (_AVPro) e.rgb = pow(e.rgb, 2.2);

            o.Alpha = e.a;
            o.Emission = e * _Emission;
		}
		    ENDCG
	}
	FallBack "Diffuse"
}