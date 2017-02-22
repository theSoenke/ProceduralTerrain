Shader "Custom/TriPlanarBump" {
	Properties{
		_Top("Top", 2D) = "white" {}
		_TopBump("Normalmap", 2D) = "bump" {}
		_Side("Side", 2D) = "white" {}
		_SideBump("Normalmap", 2D) = "bump" {}
		_Bottom("Bottom", 2D) = "white" {}
		_BottomBump("Normalmap", 2D) = "bump" {}

		_Tiling("Tiling", Float) = .5
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
		sampler2D _SideBump, _TopBump, _BottomBump;
		float _Tiling;

		struct Input {
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		void surf(Input IN, inout SurfaceOutput o) {
			float3 worldNormal = WorldNormalVector(IN, float3(0, 0, 1));
			float3 projNormal = saturate(pow(worldNormal * 1.4, 4));

			float3 x = tex2D(_Side, frac(IN.worldPos.zy * _Tiling)) * abs(worldNormal.x);
			float3 z = tex2D(_Side, frac(IN.worldPos.xy * _Tiling)) * abs(worldNormal.z);

			float3 y = 0;
			if (worldNormal.y > 0) {
				// top
				y = tex2D(_Top, frac(IN.worldPos.zx * _Tiling)) * abs(worldNormal.y);
			}
			else {
				// bottom
				y = tex2D(_Bottom, frac(IN.worldPos.zx * _Tiling)) * abs(worldNormal.y);
			}

			fixed3 n0 = UnpackNormal(tex2D(_TopBump, IN.worldPos.xz / _Tiling));
			fixed3 n1 = UnpackNormal(tex2D(_SideBump, IN.worldPos.xy / _Tiling));
			fixed3 n2 = UnpackNormal(tex2D(_BottomBump, IN.worldPos.zy / _Tiling));

			half3 normalResult = lerp(n0, n1, projNormal.z);
			normalResult = lerp(normalResult, n2, projNormal.x);

			o.Albedo = z;
			o.Albedo = lerp(o.Albedo, x, projNormal.x);
			o.Albedo = lerp(o.Albedo, y, projNormal.y);

			o.Normal = normalize(normalResult);
		}
		ENDCG
	}
	Fallback "Diffuse"
}