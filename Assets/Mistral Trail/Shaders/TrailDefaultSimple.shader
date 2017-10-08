/*
	This Shader is Specifically For Mistral Trail System. 
	A Simplified yet performance-enhanced version of the TrailDefault. 

	Author: Jingping Yu. 
	RTX: joshuayu. 

	If you have any problem ... please try harder to figure it out by yourself :). 
*/

Shader "Mistral/Trail/Trail Default Simple" 
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

				#include "UnityCG.cginc"

				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform fixed4 _TintColor;
				uniform float _InvFade;

				struct VertexInput 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct VertexOutput 
				{
					float4 pos : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				VertexOutput vert (VertexInput v)
				{
					VertexOutput o;

					o.pos = UnityObjectToClipPos(v.vertex);

					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);

					return o;
				}

				
				fixed4 frag (VertexOutput i) : SV_Target
				{
					return 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
				}

				ENDCG 
			}
		}	
	}
}
