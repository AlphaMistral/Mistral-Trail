/*
 ### Mistral ShowCase ###
 Author: Jingping Yu
 RTX: joshuayu
 Created on: 2017/07/10
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mistral.Utility.MathX;

namespace Mistral.ShowCase
{
	/// <summary>
	/// Move a transform along a trail defined by several control points. 
	/// </summary>
	public class MoveAlongTrail : MonoBehaviour 
	{
		#region Public Variables

		/// <summary>
		/// The Vital Points on the Curve.
		/// Must Be 4 or more! 
		/// </summary>
		public List<Vector3> controlPoints;

		/// <summary>
		/// Amount of additional points between control points. 
		/// </summary>
		public int precision;

		/// <summary>
		/// The Speed of the movement. 
		/// m/s
		/// </summary>
		public float speed;

		#endregion

		#region Priave Variables

		/// <summary>
		/// The B-Spline Curve. 
		/// </summary>
		private List<Vector3> curve;

		/// <summary>
		/// If the distane is smaller EPS then two points are regarded to be the same. 
		/// </summary>
		private static float EPS = 0.001f;

		/// <summary>
		/// The current point.
		/// </summary>
		private int currentIdx;

		/// <summary>
		/// Faster Faster Faster! - Carla in Evolve! 
		/// </summary>
		private Transform m_transform;

		private float timeElapsed;

		private float timeTotal;
		#endregion

		#region MonoBehaviours

		private void Start ()
		{
			if (controlPoints.Count < 4)
			{
				Debug.Log("控制点的数量不能少于4个! MoveAlongTrail组件已经被删除! ");
				DestroyImmediate(this);
			}
			m_transform = transform;
			curve = B_Spline.GeneratePoints(controlPoints, precision);
			m_transform.position = curve[0];
			currentIdx = 0;
			timeElapsed = 0f;
			timeTotal = Vector3.Distance(curve[0], curve[1]) / speed;
		}

		private void Update()
		{
			m_transform.position = Vector3.Lerp(curve[currentIdx], curve[currentIdx + 1], timeElapsed / timeTotal);
			if (Vector3.Distance(m_transform.position, curve[currentIdx + 1]) <= EPS)
			{
				m_transform.position = curve[currentIdx + 1];
				timeElapsed = 0f;
				currentIdx++;
				if (currentIdx == curve.Count - 1)
				{
					DestroyImmediate(this);
					return;
				}
				timeTotal = Vector3.Distance(curve[currentIdx], curve[currentIdx + 1]) / speed;
			}
			timeElapsed += Time.deltaTime;
		}

		#endregion

		#region Private Methods



		#endregion
	}

}
