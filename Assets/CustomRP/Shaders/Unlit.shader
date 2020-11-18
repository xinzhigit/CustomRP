Shader "Custom RP/Unlit" {
	Properties {
		_BaseColor("Color", color) = (1.0, 1.0, 1.0, 1.0)
		_BaseMap("Texture", 2D) = "white" {}
		[Toggle(_CLIPING)] _Cliping("Alpha Cliping", Float) = 0
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite("Z Write", Float) = 1
	}

	SubShader {
		Pass {
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			HLSLPROGRAM
			#pragma shader_feature _CLIPING

			#pragma multi_compile_instancing

			#pragma vertex Vert
			#pragma fragment Frag

			#include "Unlit.hlsl"
			ENDHLSL
		}
	}
}
