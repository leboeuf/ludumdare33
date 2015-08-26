Shader "Custom/AdditiveMultiplied" {
	Properties {
		_Multiply ("Multiply", Range(0,100)) = 1.0
		_Color ("Color", Color) = (1.0,1.0,1.0,1.0)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Blend One One
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alpha
		#pragma target 3.0
		
		sampler2D _MainTex;
		half _Multiply;
		half4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * _Multiply * _Color.rgb;
			o.Alpha = c.a;
			if(c.a < 0.5 || dot(o.Albedo,o.Albedo) < 0.1) discard;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
