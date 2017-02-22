Shader "Custom/TriPlanar" {
	Properties{
		_Top("Top", 2D) = "white" {}
		_Side("Side", 2D) = "white" {}
		_Bottom("Bottom", 2D) = "white" {}

		_Shininess("Shininess", Float) = 0.1
		_Tiling("Tiling", Float) = 0.5
	}

	SubShader{

		Tags{
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}

		Cull Back
		ZWrite On

		CGPROGRAM
		#pragma surface surf BlinnPhong

		sampler2D _Side, _Top, _Bottom;
		half _Tiling, _Shininess;

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			float3 projNormal = saturate(pow(IN.worldNormal * 1.4, 4));

			float3 x = tex2D(_Side, frac(IN.worldPos.zy * _Tiling)) * abs(IN.worldNormal.x);
			float3 z = tex2D(_Side, frac(IN.worldPos.xy * _Tiling)) * abs(IN.worldNormal.z);

			float3 y = 0;
			if (IN.worldNormal.y > 0) {
				// top
				y = tex2D(_Top, frac(IN.worldPos.zx * _Tiling)) * abs(IN.worldNormal.y);
			}
			else {
				// bottom
				y = tex2D(_Bottom, frac(IN.worldPos.zx * _Tiling)) * abs(IN.worldNormal.y);
			}

			float3 tex = z;
			tex = lerp(tex, x, projNormal.x);
			tex = lerp(tex, y, projNormal.y);
			
			o.Albedo = tex;

			o.Specular = _Shininess;
		}
		ENDCG
	}
	Fallback "Diffuse"
}