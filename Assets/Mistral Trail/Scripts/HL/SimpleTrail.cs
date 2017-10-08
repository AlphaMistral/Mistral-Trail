/*
 ### Mistral Trail System ###
 Author: Jingping Yu
 RTX: joshuayu
 Created on: 2017/07/08
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mistral.Effects.Trail
{
	/// <summary>
	/// Creates a simple and continuous trail. 
	/// </summary>
	public class SimpleTrail : TrailBase
	{
		#region Public Variables

		public float minVertexDistance = 0.1f;
		public int maxPointNumber = 50;

		#endregion

		#region Private Variables

		private Vector3 lastPosition;
		private float distanceMoved;

		#endregion

		#region MonoBehaviours

		protected override void Start()
		{
			base.Start();
			lastPosition = m_transform.position;
		}

		protected override void Update()
		{
			if (isEmitting)
			{
				distanceMoved += Vector3.Distance(m_transform.position, lastPosition);
				
				if (distanceMoved != 0 && distanceMoved >= minVertexDistance)
				{
					AddPoint(new TrailPoint(), m_transform.position);
					distanceMoved = 0.0f;
				}
				lastPosition = m_transform.position;
			}
			base.Update();
		}

		#endregion

		#region Override Methods

		protected override void OnStartEmit()
		{
			lastPosition = m_transform.position;
			distanceMoved = 0;
		}

		protected override void OnTranslate(Vector3 trans)
		{
			lastPosition += trans;
		}

		protected override int GetMaxPoints()
		{
			return maxPointNumber;
		}

		#endregion

	}

}