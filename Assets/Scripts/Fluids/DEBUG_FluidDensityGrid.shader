Shader "Custom/DEBUG_FluidDensityGrid" {
	Properties {
		_MainTex ("Main (RGB)", 2D) = "white" {}
		_FluidDensityTex ("FluidDensity (RGB)", 2D) = "white" {}
	}
SubShader {
	Pass{
		ZTest Always Cull Off ZWrite Off
		
		CGPROGRAM
		#pragma vertex vert_img
		#pragma fragment frag
	    #pragma target 3.0   
		#include "UnityCG.cginc"
		
		sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		sampler2D _FluidDensityTex;
		
		half4 frag (v2f_img i) : SV_Target
		{
			float2 uv = i.uv;
			#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				uv.y = 1-uv.y;
			#endif
			return tex2D(_FluidDensityTex, uv);
		}
		ENDCG
	} 
}
}
