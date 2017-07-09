/*
 ### Mistral Trail System ###
 Author: Jingping Yu
 RTX: joshuayu
 Created on: 2017/07/08
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mistral.Utility.DataStructure;

namespace Mistral.Effects.Trail
{
	/// <summary>
	/// Creates a smooth trail with discrete control points. 
	/// Currently SmoothTrail is not optimized and thus if and performance issue is detected, 
	/// please turn it off or convert to other trail types. 
	/// </summary>
	public class SmoothTrail : TrailBase 
	{
		#region SubClasses

		/// <summary>
		/// Newly generated control points during runtime to ensure smoothness. 
		/// </summary>
		public class AdditionalPoint
		{
			Vector3 position;
			Vector3 forward;
		}

		#endregion

		#region Public Variables

		public float minVertexDistance = 0.1f;

		/// <summary>
		/// Please note that since additional control points are automatically added during runtime. 
		/// It is unnecessary to assign a large point number. 
		/// </summary>
		public int maxPointNumber = 15;

		#endregion

		#region Private Variables

		private Vector3 lastPosition;
		private float distanceMoved;
		private RingBuffer<AdditionalPoint> controlPoints;

		#endregion

		#region MonoBehaviours

		protected void Start()
		{
			base.Start();
			lastPosition = m_transform.position;
		}

		protected void Update()
		{
			
		}

		#endregion

		#region Override Methods

		protected override int GetMaxPoints()
		{
			return maxPointNumber;
		}

		#endregion
	}
}
