Shader "Flat/RoundedCorner"
{
	Properties
	{
		[KeywordEnum(DEFAULT, OUTLINE, SEPARATE)]
		_TYPE("Type", Float) = 0
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _TYPE_DEFAULT _TYPE_OUTLINE _TYPE_SEPARATE

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				
				#if defined(_TYPE_OUTLINE) || defined(_TYPE_SEPARATE)
				float2 texcoord3 : TEXCOORD3;
				#endif
				
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float2 texcoord2 : TEXCOORD2;
				
				#if defined(_TYPE_OUTLINE) || defined(_TYPE_SEPARATE) 
				float2 texcoord3 : TEXCOORD3;
				#endif
				
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float4 worldPosition : TEXCOORD4;
			};
			
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPosition = v.vertex;
				o.uv = v.uv;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				
				#if defined(_TYPE_OUTLINE) || defined(_TYPE_SEPARATE)
				o.texcoord3 = v.texcoord3;
				#endif
				
				o.color = v.color;
				return o;
			}
			
			float4 _ClipRect;

			// 負数:0, 正数:1 に変換
			// 条件を見たしているかどうかチェック
			#define IS(value) ceil(saturate(value))
			// a がb よりも小さいかどうか
			#define IS_SMALL(a, b) IS( b - a)
			
			fixed4 Circle(float2 uv, fixed4 baseColor, fixed4 targetColor, fixed radius, fixed width, fixed height, fixed4 flag)
			{
				half4 orig = baseColor;
				float r = min(width, height) * radius;
				float2 XY = float2(uv.x * width, uv.y * height);
				
				// Calc Distance from each center of circle
				// LeftTop, Center:(r, r)
				float d_lt = (XY.x - r) * (XY.x - r) + (XY.y - r) * (XY.y - r);
				// LeftBot, Center:(r, h - r)
				float d_lb = (XY.x - r) * (XY.x - r) + (XY.y- (height - r)) * (XY.y - (height - r));
				// RightTop, Center:(w - r, r)
				float d_rt = (XY.x - (width - r)) * (XY.x - (width - r)) + (XY.y - r) * (XY.y - r);
				// RightBot, Center:(w - r, h - r)
				float d_rb = (XY.x - (width - r)) * (XY.x - (width - r)) + (XY.y - (height - r)) * (XY.y - (height - r));
				
				d_lb *= saturate(flag % 10);
				flag = floor(flag / 10);
				d_lt *= saturate(flag % 10);
				flag = floor(flag / 10);
				d_rb *= saturate(flag % 10);
				flag = floor(flag / 10);
				d_rt *= saturate(flag);
				
				float isNotCorner = 
					IS(IS_SMALL(r, XY.x) + IS_SMALL(XY.x, (width - r)) - 1) // r < x < 1-r
					+ IS(IS_SMALL(r, XY.y) + IS_SMALL(XY.y, (height - r)) - 1); // r < y < 1-r
				
				float left = lerp(
					lerp(1, 0, IS(d_lt - r * r)),
					lerp(1, 0, IS(d_lb - r * r)),
					IS(uv.y > 0.5)
				);
				float right = lerp(
					lerp(1, 0, IS(d_rt - r * r)),
					lerp(1, 0, IS(d_rb - r * r)),
					IS(uv.y > 0.5)
				);
				fixed v = lerp( 
					lerp(left, right, IS(uv.x > 0.5)),
					1,
					IS(isNotCorner) // r < x < 1-r && r < y < 1-r
				);
				
				return lerp(baseColor, targetColor, v);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed radius = i.texcoord1.x; 
				
				fixed width = i.texcoord2.x;
				fixed height = i.texcoord2.y;
				
				#ifdef _TYPE_OUTLINE
				fixed outline = i.texcoord3.x;
				fixed4 outlineColor = half4(0, 0, 0, 1);
				
				fixed outlineColorData = i.texcoord3.y;
				outlineColor.r = frac(i.texcoord3.y) * 10;
				outlineColor.g = floor(outlineColorData) % 1000 / 100;
				outlineColorData = floor(outlineColorData / 1000);
				outlineColor.b = outlineColorData / 100;

				fixed4 color = Circle(i.uv, half4(0, 0, 0, 0), outlineColor, radius, width, height, i.texcoord1.y);
				
				// Outlineの最低幅
				float r = min(width, height) * outline;
				
				fixed2 uv = fixed2(
					lerp(-r / width, 1 + r / width, i.uv.x), 
					lerp(-r / height, 1 + r / height, i.uv.y) 
				);
				fixed outlineX = (r / width) / (1 + r / width);
				fixed outlineY = (r / height) / (1 + r / height);
				// Inner outline
				color = Circle(uv, color, i.color, radius, width - width * outline * 2, height - height * outline * 2, i.texcoord1.y);
				
				color.rgb = lerp(
					color.rgb, 
					outlineColor,
					saturate(
						(frac(i.uv.x * (1 + outlineX)) < outlineX) + 
						(frac(i.uv.y * (1 + outlineY)) < outlineY)
					)
				);
				
				#elif _TYPE_SEPARATE
				
				fixed4 color = Circle(i.uv, half4(0, 0, 0, 0), i.color, radius, width, height, i.texcoord1.y);
				
				fixed ratio = i.texcoord3.x;
				
				fixed colorData = i.texcoord3.y;
				fixed4 separateColor = half4(0, 0, 0, 1);
				separateColor.r = frac(i.texcoord3.y) * 10;
				separateColor.g = floor(colorData) % 1000 / 100;
				colorData = floor(colorData / 1000);
				separateColor.b = colorData / 100;
				
				color.rgb = lerp(
					separateColor,
					color.rgb, 
					IS_SMALL(i.uv.y, ratio)
				);
				
				#else
				
				fixed4 color = Circle(i.uv, half4(0, 0, 0, 0), i.color, radius, width, height, i.texcoord1.y);
				
				#endif
				
				color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
				
				return color;
			}
			ENDCG
		}
	}
	
	//CustomEditor "Yorozu.FlatUI.Tool.FlatShaderGUI"
}