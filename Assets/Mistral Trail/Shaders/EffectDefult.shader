// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader  "PhotonShader/Effect/Default" 
{
    Properties
	{
		[HideInInspector]_AlphaCtrl("AlphaCtrl",range(0,1)) = 1
		[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] 		_SrcFactor ("SrcFactor()", Float) = 5
		[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] 		_DstFactor ("DstFactor()", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)] 							_CullMode ("消隐模式(CullMode)", int) = 0
		[Enum(LessEqual,4,Always,8)]									_ZTestMode ("深度测试(ZTest)", int) = 4
		//[Toggle] _ZWrite ("写入深度(ZWrite)", int) = 0
		
		[Toggle] _RgbAsAlpha ("颜色输出至透明(RgbAsAlpha)", int) = 0

        _Color ("Color", Color) = (1,1,1,1)
        _Multiplier	("亮度",range(1,100)) = 1
		
        _MainTex ("MainTex", 2D) = "white" {}
        _MainTexRot ("Tex rotation", Float ) = 0
        _MainTexX ("TexUscroll", Float ) = 0
        _MainTexY ("TexVscroll", Float ) = 0
		
        _MaskTex ("mask", 2D) = "white" {}
        _MaskTexRot ("mask_rotation", Float ) = 0
        _MaskTexX ("maskUscroll", Float ) = 0
        _MaskTexY ("maskVscroll", Float ) = 0
		
        _DissolveTex ("dissolveTex", 2D) = "white" {}
        _Dissolve ("dissolveValue", Range(0, 1)) = 0
        _DissolveColor1("dissolveColor1",color) = (1,0,0,1)
        _DissolveColor2("dissolveColor2",color) = (0,0,0,1)
        _DissolveRot ("dissolve rotation", Float ) = 0
		
        _FlowTex ("flow", 2D) = "black" {}
        _FlowTexRot ("flow rotation", Float ) = 0
        _FlowScale ("flow value", Range(0, 2)) = 0
        _FlowTexX ("flowVscroll", Float ) = 0
        _FlowTexY ("flowUscroll", Float ) = 0

    }
	
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

		Blend [_SrcFactor] [_DstFactor]
		Cull [_CullMode]
		//ZWrite [_ZWrite]
		ZWrite off
		ZTest [_ZTestMode]

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma target 2.0
			#include "UnityCG.cginc"
			#pragma multi_compile_instancing
			#pragma multi_compile MaskTex_Off  		MaskTex_On
			#pragma multi_compile DissolveTex_Off  	DissolveTex_On
			#pragma multi_compile FlowTex_Off  		FlowTex_On
			#pragma multi_compile MultiplyBlend_Off	MultiplyBlend_On
			#pragma multi_compile RT_PASS_OFF RT_PASS_ON


			float2 TransFormUV(float2 argUV,float4 argTM)
			{
				float2 result = argUV.xy * argTM.xy + (argTM.zw + float2(0.5,0.5) - argTM.xy * 0.5);
				return result;
			}

			half2 RotateUV(half2 uv,half uvRotate)
			{
				half2 outUV;
				half s;
				half c;
				s = sin(uvRotate/57.2958);
				c = cos(uvRotate/57.2958);
				
				outUV = uv - half2(0.5f, 0.5f);
				outUV = half2(outUV.x * c - outUV.y * s, outUV.x * s + outUV.y * c);
				outUV = outUV + half2(0.5f, 0.5f);
				return outUV;
			}

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct fragData
			{
				float4 uv12 : TEXCOORD0;
				float4 uv34 : TEXCOORD1;

				float4 vertex : SV_POSITION;
				float4 vertexColor : COLOR;

			};

			float _AlphaCtrl;

			float _Multiplier;

            sampler2D _MainTex;
			
			float _MainTexRot;
            float _MainTexX;
            float _MainTexY;

            #if MaskTex_On
				sampler2D _MaskTex;
				float4 _MaskTex_ST;
				float _MaskTexRot;
				float _MaskTexX;
				float _MaskTexY;
			#endif

			#if DissolveTex_On	
				sampler2D _DissolveTex ;
				float4 _DissolveTex_ST;
				float _Dissolve ;
				float _DissolveRot ;
				fixed4 _DissolveColor1;
				fixed4 _DissolveColor2;
			#endif

			#if FlowTex_On
				sampler2D _FlowTex ;
				float4 _FlowTex_ST;
				float _FlowTexRot;
				float _FlowScale ;
				float _FlowTexX;
				float _FlowTexY;
			#endif

			const float threshold = 0.5;

			float _SrcFactor;
			float _RgbAsAlpha;

			#if defined (INSTANCING_ON)

			UNITY_INSTANCING_CBUFFER_START(Props)
            	UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color);
            	UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST);
        	UNITY_INSTANCING_CBUFFER_END

        	#else
        		fixed4 _Color;
        		float4 _MainTex_ST;
        	#endif

			fragData vert (appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				fragData o = (fragData)0;
				o.vertex =  UnityObjectToClipPos(v.vertex);

				#if defined (INSTANCING_ON)
				o.uv12.xy = TransFormUV (v.uv ,UNITY_ACCESS_INSTANCED_PROP(_MainTex_ST));
				#else
				o.uv12.xy = TransFormUV (v.uv ,_MainTex_ST);
				#endif
				o.uv12.xy = RotateUV(o.uv12.xy,_MainTexRot);
				o.uv12.xy += _Time.z * float2(_MainTexX,_MainTexY);

				#if MaskTex_On
					o.uv12.zw = TransFormUV(v.uv,_MaskTex_ST);
					o.uv12.zw = RotateUV(o.uv12.zw,_MaskTexRot);
					o.uv12.zw += _Time.z * float2(_MaskTexX,_MaskTexY);
				#endif

				#if DissolveTex_On	
					o.uv34.xy = TransFormUV(v.uv ,_DissolveTex_ST);
					o.uv34.xy = RotateUV(o.uv34.xy,_DissolveRot);
				#endif

				#if FlowTex_On
					o.uv34.zw = TransFormUV(v.uv,_FlowTex_ST);
					o.uv34.zw = RotateUV(o.uv34.zw,_FlowTexRot);
					o.uv34.zw +=  _Time.z * float2(_FlowTexX,_FlowTexY) / 100000;
				#endif
				#if defined (INSTANCING_ON)
				o.vertexColor = v.vertexColor * UNITY_ACCESS_INSTANCED_PROP(_Color) * _Multiplier;
				#else
				o.vertexColor = v.vertexColor * _Color * _Multiplier;
				#endif
				//o.vertexColor.a = saturate(o.vertexColor.a);

				return o;
			}
			
			fixed4 frag (fragData i) : SV_Target
			{
	
				#if DissolveTex_On
					fixed4 dissolveColor = tex2D(_DissolveTex, i.uv34.xy);
					float clipValue = (dissolveColor.r+(_Dissolve*-1.2+0.6)) - 0.6;
					clip(clipValue);
				#endif

				float2 flowUV = i.uv12.xy;
				#if FlowTex_On
					fixed4 flowColor = tex2D(_FlowTex, i.uv34.zw);
					flowUV = i.uv12.xy + (flowColor.xy - 0.5) * _FlowScale;
				#endif

				fixed4 texColor = tex2D(_MainTex, flowUV);

				fixed4 result = (fixed4)1;
				result = texColor;

				#if DissolveTex_On
					clipValue = clamp(clipValue * 3  ,0,1);
					fixed4 dissColor = lerp(_DissolveColor1,_DissolveColor2,clipValue  > 0.2);
					clipValue = clamp(clipValue  + (_Dissolve  < 0.001),0,1);
					result.rgb = lerp(dissColor,texColor,clipValue).rgb;
				#endif

				#if MaskTex_On
					fixed4 maskColor = tex2D(_MaskTex, i.uv12.zw);
					result.a *= maskColor.r;
				#endif

				float gray = LinearRgbToLuminance(result.rgb);
				float aa[2] = {result.a,gray};
				result.a *= aa[_RgbAsAlpha];
				
				result *= i.vertexColor;		

				# if MultiplyBlend_On
					fixed4 multiplyColor = lerp(half4(1,1,1,1), result, result.a);
					result = lerp(result, multiplyColor ,_SrcFactor == 0);
				#endif
				
				result.a *= _AlphaCtrl;


				return result;
			}
			ENDCG
		}
	}

}
