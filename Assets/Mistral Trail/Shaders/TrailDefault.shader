/*
	This Shader is Specifically For Mistral Trail System. 
	Default Shading - Quite Similar to Particle/Additive! 
	Good Visual Quality but may suffer performance issues. 
	
	Author: Jingping Yu. 
	RTX: joshuayu. 

	If you have any problem ... please try harder to figure it out by yourself :). 
*/

Shader "Mistral/Trail/Trail Default" 
{
	Properties 
	{
		_TintColor ("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01, 3.0)) = 1.0
	}

	Category 
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }

		Blend SrcAlpha One
		ColorMask RGB
		Cull Off 
		Lighting Off 
		ZWrite Off
		
		SubShader 
		{
			Pass 
			{
			
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_particles
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform fixed4 _TintColor;

				uniform sampler2D_float _CameraDepthTexture;
				uniform float _InvFade;

				struct VertexInput 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct VertexOutput 
				{
					float4 pos : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;

					UNITY_FOG_COORDS(1)

					#ifdef SOFTPARTICLES_ON

					float4 projPos : TEXCOORD2;

					#endif

					UNITY_VERTEX_OUTPUT_STEREO
				};

				VertexOutput vert (VertexInput v)
				{
					VertexOutput o;

					UNITY_SETUP_INSTANCE_ID(v);

					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					o.pos = UnityObjectToClipPos(v.vertex);

					#ifdef SOFTPARTICLES_ON

					o.projPos = ComputeScreenPos (o.vertex);

					COMPUTE_EYEDEPTH(o.projPos.z);

					#endif

					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);

					UNITY_TRANSFER_FOG(o,o.vertex);

					return o;
				}

				
				fixed4 frag (VertexOutput i) : SV_Target
				{
					#ifdef SOFTPARTICLES_ON

					float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
					float partZ = i.projPos.z;
					float fade = saturate (_InvFade * (sceneZ-partZ));
					i.color.a *= fade;

					#endif
					
					fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);

					UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0));

					return col;
				}

				ENDCG 
			}
		}	
	}
}
