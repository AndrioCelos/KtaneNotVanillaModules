// This is a modified version of the Mobile Diffuse shader from KTaNE,
// which only shows part of the model within a vertex coordinate range.
// It is designed to be used with models that have multiple disconnected parts;
// using it with a fully-connected model may produce strange geometry.

Shader "Mobile Diffuse with Cutoff" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		[Enum(Bottom, 0, Top, 1)] ShowTopHalf ("Show Half", Int) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 150

		CGPROGRAM
			// Mobile improvement: noforwardadd
			// http://answers.unity3d.com/questions/1200437/how-to-make-a-conditional-pragma-surface-noforward.html
			// http://gamedev.stackexchange.com/questions/123669/unity-surface-shader-conditinally-noforwardadd
			#pragma surface surf Lambert
			#pragma vertex vert

			sampler2D _MainTex;
			int ShowTopHalf;

			struct Input {
				float2 uv_MainTex;
			};

			void surf (Input IN, inout SurfaceOutput o) {
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}

			void vert (inout appdata_full v) {
				if (ShowTopHalf ? (v.vertex.z < 0) : (v.vertex.z > 0))
					v.vertex.xyz = float3(0, 0, 0);
			}
		ENDCG
	}

	Fallback "Mobile/VertexLit"
}
