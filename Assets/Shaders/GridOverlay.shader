Shader "Custom/GridOverlay" {
	Properties {
		_MaskTex ("Mask", 2D) = "white" {}
		_GridTex ("Grid", 2D) = "white" {}
		_LandTex ("Land", 2D) = "white" {}
	}

	SubShader {
		Tags {"Queue"="Transparent+5" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
		
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		
		Pass {  
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord0 : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 texcoord0 : TEXCOORD0;
					half2 texcoord1 : TEXCOORD1;
					UNITY_FOG_COORDS(1)
				};

				sampler2D _MaskTex, _GridTex, _LandTex;
				float4 _MaskTex_ST, _GridTex_ST;
				
				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord0 = TRANSFORM_TEX(v.texcoord0, _MaskTex);
					o.texcoord1 = TRANSFORM_TEX(v.texcoord1, _GridTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 mask = tex2D(_MaskTex, i.texcoord0);
					fixed4 col = mask.a * tex2D(_GridTex, i.texcoord1);
					col += (1-mask.a) * tex2D(_LandTex, i.texcoord1);
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
			ENDCG
		}
	}

}
