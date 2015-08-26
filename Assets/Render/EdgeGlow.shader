
Shader "Hidden/EdgeGlow" { 
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}
	
SubShader {
	pass {
	  ZTest Always Cull Off ZWrite Off

      CGPROGRAM
	  #pragma target 3.0   
      #pragma vertex vertD
      #pragma fragment fragD
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv[5] : TEXCOORD0;
	};
	
	struct v2fd {
		float4 pos : SV_POSITION;
		float2 uv[2] : TEXCOORD0;
	};

	sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;

	sampler2D _CameraDepthNormalsTexture;
	sampler2D_float _CameraDepthTexture;

	uniform half _SampleDistance;
	uniform float _Exponent;
	uniform float _EdgeMultiplier;

	struct v2flum {
		float4 pos : SV_POSITION;
		float2 uv[3] : TEXCOORD0;
	};
		
	v2fd vertD( appdata_img v )
	{
		v2fd o;
		o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		
		float2 uv = v.texcoord.xy;
		o.uv[0] = uv;
		
		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			uv.y = 1-uv.y;
		#endif
		
		o.uv[1] = uv;
		
		return o;
	}

	float4 fragD(v2fd i) : SV_Target 
	{	
		// inspired by borderlands implementation of popular "sobel filter"

		float centerDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv[1]));
		float4 depthsDiag;
		float4 depthsAxis;

		float2 uvDist = _SampleDistance * _MainTex_TexelSize.xy;

		depthsDiag.x = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]+uvDist)); // TR
		depthsDiag.y = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]+uvDist*float2(-1,1))); // TL
		depthsDiag.z = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]-uvDist*float2(-1,1))); // BR
		depthsDiag.w = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]-uvDist)); // BL

		depthsAxis.x = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]+uvDist*float2(0,1))); // T
		depthsAxis.y = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]-uvDist*float2(1,0))); // L
		depthsAxis.z = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]+uvDist*float2(1,0))); // R
		depthsAxis.w = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[1]-uvDist*float2(0,1))); // B

		// make it work nicely with depth based image effects such as depth of field:
		depthsDiag = (depthsDiag > centerDepth.xxxx) ? depthsDiag : centerDepth.xxxx;
		depthsAxis = (depthsAxis > centerDepth.xxxx) ? depthsAxis : centerDepth.xxxx;

		depthsDiag -= centerDepth;
		depthsAxis /= centerDepth;

		const float4 HorizDiagCoeff = float4(1,1,-1,-1);
		const float4 VertDiagCoeff = float4(-1,1,-1,1);
		const float4 HorizAxisCoeff = float4(1,0,0,-1);
		const float4 VertAxisCoeff = float4(0,1,-1,0);

		float4 SobelH = depthsDiag * HorizDiagCoeff + depthsAxis * HorizAxisCoeff;
		float4 SobelV = depthsDiag * VertDiagCoeff + depthsAxis * VertAxisCoeff;

		float SobelX = dot(SobelH, float4(1,1,1,1));
		float SobelY = dot(SobelV, float4(1,1,1,1));
		float Sobel = sqrt(SobelX * SobelX + SobelY * SobelY);

		Sobel = pow(saturate(Sobel), _Exponent);
		float3 color = tex2D(_MainTex, i.uv[0].xy);

		return float4(color + _EdgeMultiplier * Sobel * color, 1.0);
	}
	
	ENDCG 
	}
}
Fallback off
	
} // shader
