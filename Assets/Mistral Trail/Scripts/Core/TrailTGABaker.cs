/*
 ### Mistral Trail System ###
 Author: Jingping Yu
 RTX: joshuayu
 Created on: 2017/08/04
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VacuumShaders.TextureExtensions;

namespace Mistral.Effects.Trail
{
	#region SubClasses

	/// <summary>
	/// This Class Serves as a Collection of the necessary parameters to use in TGA Baker. 
	/// </summary>
	public class TGABakerParameter
	{
		#region Public Variables

		/// <summary>
		/// The size of the trail over lifetime. 
		/// This will influence the corresponding texcoord on the final TGA Texture. 
		/// It changes tiling rather than directly clamping. 
		/// </summary>
		public AnimationCurve sizeOverLife;

		/// <summary>
		/// The color of the trail over lifetime.
		/// Actually this will influence the color only. 
		/// </summary>
		public Gradient colorOverLife;

		/// <summary>
		/// #Material Property# Main Texture (RGB). 
		/// </summary>
		public Texture2D mainTexture;

		/// <summary>
		/// #Material Property# Main Texture Color Tint (RGBA). 
		/// Please note that the ALPHA channel is in USE! 
		/// </summary>
		public Color mainTextureTint;

		/// <summary>
		/// Width should not be too smaller than that of the _MainTex. 
		/// Otherwise Bilinear Filtering may FAIL! 
		/// </summary>
		public int width;

		/// <summary>
		/// The height of the generated Texture. Please note literally this should be smaller than width ... 
		/// </summary>
		public int height;

		#endregion

		#region ctor

		public TGABakerParameter()
		{
			
		}

		/// <summary>
		/// Build From Manually Inputing Mat Params. 
		/// </summary>
		/// <param name="sol">Sol.</param>
		/// <param name="col">Col.</param>
		/// <param name="mainT">Main t.</param>
		/// <param name="mainTT">Main T.</param>
		public TGABakerParameter(AnimationCurve sol, Gradient col, Texture2D mainT, Color mainTT, int w = 256, int h = 64)
		{
			SetCurves(sol, col);
			SetMaterial(mainT, mainTT);
			SetResolution(w, h);
		}

		/// <summary>
		/// IMPORTANT: The Shader of the Material is Recommended to be Particle/Additive! 
		/// If not, please at least make sure that you have a _MainTex and a _TintColor;
		/// </summary>
		/// <param name="sol">Sol.</param>
		/// <param name="col">Col.</param>
		/// <param name="mat">Mat.</param>
		public TGABakerParameter(AnimationCurve sol, Gradient col, Material mat, int w = 256, int h = 64)
		{
			SetCurves(sol, col);
			SetMaterial(mat);
			SetResolution(w, h);
		}

		#endregion

		#region Public Methods

		public void SetCurves(AnimationCurve sol, Gradient col)
		{
			sizeOverLife = sol;
			colorOverLife = col;
		}

		public void SetMaterial(Texture2D mainT, Color mainTT)
		{
			mainTexture = mainT;
			mainTextureTint = mainTT;
		}

		/// <summary>
		/// IMPORTANT: The Shader of the Material is Recommended to be Particle/Additive! 
		/// If not, please at least make sure that you have a _MainTex and a _TintColor. 
		/// </summary>
		/// <param name="mat">Mat.</param>
		public void SetMaterial(Material mat)
		{
			mainTexture = mat.GetTexture("_MainTex") as Texture2D;
			mainTextureTint = mat.GetColor("_TintColor");
		}

		public void SetResolution(int w, int h)
		{
			width = w;
			height = h;
		}

		#endregion
	}

	#endregion

	#if UNITY_EDITOR

	/// <summary>
	/// This Class Serves as a Kernal Offline TGA Texture Baker. 
	/// Singleton. Static. Unity Editor Only. No Performance Consideration. 
	/// </summary>
	public static class TrailTGABaker 
	{
		#region Public Variables

		/// <summary>
		/// The parameters required to bake the texture. 
		/// </summary>
		public static TGABakerParameter parameter;

		#endregion

		#region Private Variables

		/// <summary>
		/// The generated texture.
		/// Please note that the texture itself is not compressed. 
		/// Unity Supports DXT1 texture compression but it doesn't work on some platforms. 
		/// IIRC it doesn't work on Android. 
		/// </summary>
		private static Texture2D generatedTexture;

		/// <summary>
		/// The maximum of the size. 
		/// All the sizes must be divided by this param to be remapped to 0 - 1. 
		/// </summary>
		private static float sizeScalor;

		/// <summary>
		/// This is the material used to post-process the generated texture. 
		/// </summary>
		private static Material imageProcessor;

		#endregion

		#region ctor

		public static void Initialize(TGABakerParameter param)
		{
			parameter = param;
			sizeScalor = 0f;
			for (int i = 0; i < parameter.width; i++)
			{
				float ratio = (float)i / parameter.width;
				sizeScalor = Mathf.Max(sizeScalor, parameter.sizeOverLife.Evaluate(ratio));
			}
			imageProcessor = new Material(Shader.Find("Hidden/Mistral/Graphics/Anti-Aliasing Processor"));
		}

		#endregion

		#region Public Interfaces

		/// <summary>
		/// Generates a TGA Texture based on the parameters provided. 
		/// </summary>
		/// <returns>The texture.</returns>
		public static byte[] GenerateTexture()
		{
			ClearTexture();

			for (int i = 0; i < parameter.width; i++)
			{
				float ratio = (float)i / parameter.width;
				Color color = parameter.colorOverLife.Evaluate(ratio);
				float size = parameter.sizeOverLife.Evaluate(ratio) / sizeScalor;
				/// Pixels out of bounds should have a 0 alpha. 
				int actualHeight = parameter.height;//(int)Mathf.Ceil(size * parameter.height);
				int start = (int)Mathf.Ceil(0.5f * actualHeight * (1 - size));
				int end = (int)Mathf.Ceil(0.5f * actualHeight * (1 + size));
				for (int j = start; j < end; j++)
				{
					Vector2 uv = new Vector2((float)i / parameter.width, (float)j / actualHeight);
					//Debug.Log(uv);
					Color mainTex = parameter.mainTexture.GetPixelBilinear(uv.x, uv.y);
					Color tintedMainTex = mainTex * parameter.mainTextureTint * color;
					generatedTexture.SetPixel(i, j, tintedMainTex);
					//Debug.Log(mainTex);
				}
			}

			generatedTexture.Apply();

			RenderTexture temp = new RenderTexture(parameter.width, parameter.height, 0);

			Graphics.Blit(generatedTexture, temp, imageProcessor);

			RenderTexture.active = temp;

			generatedTexture.ReadPixels(new Rect(0, 0, parameter.width, parameter.height), 0, 0);
			generatedTexture.Apply();

			return generatedTexture.EncodeToTGA();
		}


		#endregion

		#region Private Methods

		/// <summary>
		/// Clears all the pixels in the Texture and Re-set it to parameter. 
		/// All set to black and totally transparent. 
		/// </summary>
		private static void ClearTexture()
		{
			generatedTexture = new Texture2D(parameter.width, parameter.height);
			for (int i = 0; i < parameter.width; i++)
			{
				for (int j = 0; j < parameter.height; j++)
				{
					generatedTexture.SetPixel(i, j, new Color(0f, 0f, 0f, 0f));
				}
			}
			generatedTexture.Apply();
		}

		#endregion
	}

	#endif
}
