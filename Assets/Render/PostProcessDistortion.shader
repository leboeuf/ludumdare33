// Based on: http://forum.unity3d.com/threads/horizontal-wave-distortion.295769/
Shader "Custom/PostProcess Disortion" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_DistoTex ("Texture (R,G=X,Y Distortion)", 2D) = "white" {}
	_DistoMaskTex ("Mask Texture (R)", 2D) = "white" {}
	_DistoTex_TillingOffset ("Tilling (XY); Offset (ZW)", Vector) = (1.0,1.0,0.0,0.0)
	_IntensityAndScrolling ("Intensity (XY); Scrolling (ZW)", Vector) = (0.1,0.1,1,1)
}

SubShader {	
	Pass {  
		ZTest Always Cull Off ZWrite Off	
		CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma shader_feature MASK
			#include "UnityCG.cginc"
			
			uniform sampler2D _MainTex;
			uniform sampler2D _DistoTex;
			uniform sampler2D _DistoMaskTex;
			uniform float4 _DistoTex_TillingOffset; 
			
			// x=horizontal intensity, y=vertical intensity
			// z=horizontal scrolling speed, w=vertical scrolling speed
			uniform float4 _IntensityAndScrolling;
		
			fixed4 frag (v2f_img i) : SV_Target
			{		
				float2 distouv = i.uv.xy * _DistoTex_TillingOffset.xy + _DistoTex_TillingOffset.zw;
				distouv += _Time.gg * _IntensityAndScrolling.zw; // Apply texture scrolling.
				half2 distort = tex2D(_DistoTex, distouv).xy;
				half2 offset = (distort.xy * 2 - 1) * _IntensityAndScrolling.xy;
					
				half  mask = tex2D(_DistoMaskTex, i.uv).b;				
				offset *= mask;
																				
				// get screen space position of current pixel
				half2 uv = i.uv + offset;
				half4 color = tex2D(_MainTex, uv);
				UNITY_OPAQUE_ALPHA(color.a);

				return color;
			}
		ENDCG
	}
}

}
