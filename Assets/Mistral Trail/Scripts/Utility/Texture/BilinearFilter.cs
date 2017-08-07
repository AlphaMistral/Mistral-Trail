using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mistral.Utility.Texture
{
	/// <summary>
	/// This Bilinear Filter Serves Primarily as a Texcoord Remapper ... 
	/// So Just have fun! 
	/// Singleton. Static. 
	/// </summary>
	public static class BilinearFilter 
	{
		#region Public Interfaces

		public static Color GetPixel(Texture2D tex, float s, float t)
		{
			return Color.black;
		}

		#endregion
	}

}
