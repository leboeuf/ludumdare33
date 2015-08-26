Shader "Custom/FluidDensity/Plane" {
	Properties {
		_FluidTex ("Main (RGB)", 2D) = "white" {}
		_MaskTex ("Mask (R)", 2D) = "white" {}
	}
	SubShader {
		Pass{
			Blend SrcAlpha OneMinusSrcAlpha
			Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 3.0   
			#include "UnityCG.cginc"
		
			sampler2D _FluidTex;
			float4 _FluidTex_TexelSize;
			sampler2D _MaskTex;
		
			half4 frag (v2f_img i) : SV_Target
			{
				float2 uv = float2(1, 1)-i.uv;
				#if UNITY_UV_STARTS_AT_TOP
				if (_FluidTex_TexelSize.y < 0)
					uv.y = 1-uv.y;
				#endif
				half4 color = tex2D(_FluidTex, uv);
				return half4(color.rgb, color.a * tex2D(_MaskTex, uv).r);
			}
			ENDCG
		} 
	}
}
