Shader "GrassFlow/Grass Material Shader URP" {
	Properties {
		[Space(15)]
		[HideInInspector] _CollapseStart("Grass Properties", Float) = 1
		[HDR]_Color("Grass Color", Color) = (1,1,1,1)
		bladeHeight("Blade Height", Float) = 1.0
		bladeWidth("Blade Width", Float) = 0.05
		bladeSharp("Blade Sharpness", Float) = 0.3
		seekSun("Seek Sun", Float) = 0.6
		topViewPush("Top View Adjust", Float) = 0.5
		flatnessMult("Flatness Adjust", Float) = 1.25
		[Toggle(BILLBOARD)]
		_BILLBOARD("Billboard", Float) = 1
		variance("Variances (p,h,c,w)", Vector) = (0.4, 0.4, 0.4, 0.4)
		_CollapseEnd("Grass Properties", Float) = 0

		[HideInInspector] _CollapseStart("Lighting Properties", Float) = 0
		[ToggleOff(_RECEIVE_SHADOWS_OFF)] _ReceiveShadows("Receive Shadows", Float) = 1.0
		[Toggle(SRP_PER_PIXEL_SECONDARY_LIGHTS)] _ppSecondaryLights("Per-Pixel Secondary Lights", Float) = 0
		_AO("AO", Float) = 0.25
		ambientCO("Ambient", Float) = 0.5
		ambientCOShadow("Shadow Ambient", Float) = 0.5
		edgeLight("Edge On Light", Float) = 0.4
		edgeLightSharp("Edge On Light Sharpness", Float) = 8
		_CollapseEnd("Lighting Properties", Float) = 0

		[Space(15)]
		[HideInInspector] _CollapseStart("LOD Properties", Float) = 0
		[Toggle(ALPHA_TO_MASK)]
		_ALPHA_TO_MASK("Alpha To Mask", Float) = 0
		widthLODscale("Width LOD Scale", Float) = 0.04
		grassFade("Grass Fade", Float) = 120
		grassFadeSharpness("Fade Sharpness", Float) = 8
		[HideInInspector]_LOD("LOD Params", Vector) = (20, 1.1, 0.2, 0.0)
		_CollapseEnd("LOD Properties", Float) = 0

		[Space(15)]
		[HideInInspector]_CollapseStart("Wind Properties", Float) = 0
		[HDR]windTint("windTint", Color) = (1,1,1, 0.15)
		_noiseScale("Noise Scale", Vector) = (1,1,.7)
		_noiseSpeed("Noise Speed", Vector) = (1.5,1,0.35)
		windDir  ("Wind Direction", Vector) = (-0.7,-0.6,0.1)
		windDir2 ("Secondary Wind Direction", Vector) = (0.5,0.5,1.2)
		_CollapseEnd("Wind Properties", Float) = 0

		[Space(15)]
		[HideInInspector]_CollapseStart("Bendable Settings", Float) = 0
		[Toggle(MULTI_SEGMENT)]
		_MULTI_SEGMENT("Enable Bending", Float) = 0
		bladeLateralCurve("Curvature", Float) = 0
		bladeVerticalCurve("Droop", Float) = 0
		bladeStiffness("Stiffness", Float) = 0
		_CollapseEnd("Bendable Settings", Float) = 0

		[Space(15)]
		[HideInInspector]_CollapseStart("Maps and Textures", Float) = 0
		[Toggle(SEMI_TRANSPARENT)]
		_SEMI_TRANSPARENT("Enable Alpha Clip", Float) = 0
		alphaClip("Alpha Clip", Float) = 0.25
		numTextures("Number of Textures", Int) = 1
		_MainTex("Grass Texture", 2D) = "white"{}
		[NoScaleOffset] colorMap("Grass Color Map", 2D) = "white"{}
		[NoScaleOffset] dhfParamMap("Grass Parameter Map", 2D) = "white"{}
		[NoScaleOffset] typeMap("Grass Type Map", 2D) = "black"{}
		_CollapseEnd("Maps and Textures", Float) = 0

		[HideInInspector] terrainNormalMap("Terrain Normal Map", 2D) = "black"{}
	}

	SubShader{

		//Blend[_SrcBlend][_DstBlend]
		Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" "IgnoreProjector" = "True" "Queue" = "AlphaTest"}

		// UsePass "GrassFlow/Grass Material Shader/FORWARDBASEPASS"
		pass {
			Name "ForwardBasePass"

			Blend SrcAlpha OneMinusSrcAlpha
			Tags { "RenderType" = "Transparent" "LightMode" = "UniversalForward" }

			AlphaToMask [_ALPHA_TO_MASK]
			Cull Off 

			HLSLPROGRAM

			//URP Pragma nonsense
			#define URP
			#define SRP

			#include "GrassURPInclude.cginc"

			#pragma target 4.0 
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE		

			#pragma shader_feature _RECEIVE_SHADOWS_OFF

			#pragma shader_feature_local SRP_PER_PIXEL_SECONDARY_LIGHTS

			#pragma shader_feature_local BILLBOARD
			#pragma shader_feature_local SEMI_TRANSPARENT
			#pragma shader_feature_local RENDERMODE_MESH
			#pragma shader_feature_local GRASS_EDITOR
			#pragma shader_feature_local BAKED_HEIGHTMAP
			#pragma shader_feature_local MULTI_SEGMENT

			#pragma multi_compile_instancing
			#pragma multi_compile_fog
				
			#include "GrassPrograms.cginc"

			#pragma vertex vertex_shader
			#pragma geometry geometry_shader
			#pragma fragment fragment_shader	

			ENDHLSL
		}// base pass
		
		pass {
			Name "DepthPass"

			Blend SrcAlpha OneMinusSrcAlpha
			Tags { "RenderType" = "Transparent" "LightMode" = "ShadowCaster"}

				//AlphaToMask On
				Cull Off

				HLSLPROGRAM

				//URP Pragma nonsense
				#define URP
				#define SRP
				#define SRP_SHADOWCASTER				

				#include "GrassURPInclude.cginc"

				#pragma target 4.0 
				#pragma prefer_hlslcc gles
				#pragma exclude_renderers d3d11_9x

				#pragma vertex vertex_shader
				#pragma geometry geometry_shader
				#pragma fragment fragment_depth

				#pragma multi_compile_instancing
				#pragma multi_compile_shadowcaster

				#pragma shader_feature_local RENDERMODE_MESH
				#pragma shader_feature_local BILLBOARD
				#pragma shader_feature_local SEMI_TRANSPARENT
				#pragma shader_feature_local BAKED_HEIGHTMAP
				#pragma shader_feature_local MULTI_SEGMENT

				#define SHADOW_CASTER

				#include "GrassPrograms.cginc"


				ENDHLSL
			}// depth pass

		pass {
			Name "DepthPass"

			Blend SrcAlpha OneMinusSrcAlpha
			Tags { "RenderType" = "Transparent" "LightMode" = "DepthOnly"}

			//AlphaToMask On
			Cull Off

			HLSLPROGRAM

			//URP Pragma nonsense
			#define URP
			#define SRP

			#include "GrassURPInclude.cginc"

			#pragma target 4.0 
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vertex_shader
			#pragma geometry geometry_shader
			#pragma fragment fragment_depth

			#pragma multi_compile_instancing
			#pragma multi_compile_shadowcaster

			#pragma shader_feature_local RENDERMODE_MESH
			#pragma shader_feature_local BILLBOARD
			#pragma shader_feature_local SEMI_TRANSPARENT
			#pragma shader_feature_local BAKED_HEIGHTMAP
			#pragma shader_feature_local MULTI_SEGMENT

			#define SHADOW_CASTER

			#include "GrassPrograms.cginc"

			ENDHLSL
		}// depth pass
	}
	CustomEditor "GrassFlow.GrassShaderGUI"
}
