/*
 ### Mistral MathX ###
 Author: Jingping Yu
 RTX: joshuayu
 Created on: 2017/07/10
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mistral.Utility.MathX
{
	/// <summary>
	/// Calculates Cubic Bezier Curve
	/// </summary>
	public static class Bezier 
	{
		#region Public Interface

		/// <summary>
		/// Calculates the Cubic Bezier Curve based on the interpolation T and four control points. s
		/// </summary>
		/// <returns>The cubic.</returns>
		/// <param name="t">T.</param>
		/// <param name="p0">P0.</param>
		/// <param name="p1">P1.</param>
		/// <param name="p2">P2.</param>
		/// <param name="p3">P3.</param>
		public static Vector3 CalculateCubic(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float smooth)
		{
			float u = 1 - t;
			float tt = t * t;
			float uu = u * u;
			float uuu = uu * u;
			float ttt = tt * t;
			float oms = 1 - smooth;
			Vector3 up = uuu * p0 * oms + 3 * uu * t * p1 * smooth + 3 * u * tt * p2 * smooth + ttt * p3 * oms;
			float down = uuu * oms + 3 * uu * t * smooth + 3 * u * tt * smooth + ttt * oms;
			return up / down;
		}

		#endregion
	}

}
