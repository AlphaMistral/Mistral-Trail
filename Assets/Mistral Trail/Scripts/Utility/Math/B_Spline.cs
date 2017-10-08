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
	/// This class is used to generate B_Spline Curves. 
	/// It serves as a plugin rather than a solution. 
	/// </summary>
	public static class B_Spline 
	{
		#region Public Variables

		/// <summary>
		/// From 3 to max. Please note that only the first and the last control points are guaranteed to be located on the curve. 
		/// </summary>
		public static List<Vector3> controlPoints;

		/// <summary>
		/// How many points are generated between the control points. 
		/// </summary>
		public static int precision;

		#endregion

		#region Private Variables

		/// <summary>
		/// Trisection Control Points. 
		/// </summary>
		private static List<Vector3> triSectionControlPoints;

		/// <summary>
		/// S points - Mid points of trisections around control points. 
		/// </summary>
		private static List<Vector3> sPoints;

		/// <summary>
		/// The results are stored here. 
		/// </summary>
		private static List<Vector3> results;

		#endregion

		#region Public Interface

		/// <summary>
		/// The only Public Interface to use. 
		/// </summary>
		/// <returns>The points.</returns>
		/// <param name="cp">Cp.</param>
		/// <param name="p">P.</param>
		public static List<Vector3> GeneratePoints(List<Vector3> cp, int p)
		{
			Init();

			controlPoints = cp;
			precision = p;

			GenerateAFramePoints();
			GenerateBSpline();

			return results;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Set up variables and no BS. 
		/// </summary>
		private static void Init ()
		{
			triSectionControlPoints = new List<Vector3>();
			sPoints = new List<Vector3>();
			results = new List<Vector3>();
		}

		/// <summary>
		/// Check WiKi for more info about AFrame. 
		/// A more efficient way to calcualte B Spline. 
		/// </summary>
		private static void GenerateAFramePoints()
		{
			for (int i = 1; i < controlPoints.Count; i++)
			{
				Vector3 trisectionLine = (controlPoints[i] - controlPoints[i - 1]) / 3;
				Vector3 trisection_first = controlPoints[i - 1] + trisectionLine;
				Vector3 trisection_second = controlPoints[i] - trisectionLine;

				triSectionControlPoints.Add(trisection_first);
				triSectionControlPoints.Add(trisection_second);
			}

			sPoints.Add(controlPoints[0]);

			for (int i = 1; i < controlPoints.Count - 1; i++)
			{
				Vector3 preNode = triSectionControlPoints[i * 2 - 1];
				Vector3 nxtNode = triSectionControlPoints[i * 2];

				sPoints.Add((preNode + nxtNode) / 2);
			}

			sPoints.Add(controlPoints[controlPoints.Count - 1]);
		}

		/// <summary>
		/// Final step - Cubic Bezier among the control points. 
		/// </summary>
		private static void GenerateBSpline()
		{
			for (int i = 0; i < sPoints.Count - 1; i++)
			{
				for (int j = 0; j < precision; j++)
				{
					///HYPOTHESIS: Should use (j + 1) / precision instead. 
					float t = (float)j / (float)(precision - 1);
					results.Add(Bezier.CalculateCubic(t, sPoints[i], triSectionControlPoints[i * 2], triSectionControlPoints[i * 2 + 1], sPoints[i + 1], 0.3f));
				}
			}
		}

		#endregion

	}

}
